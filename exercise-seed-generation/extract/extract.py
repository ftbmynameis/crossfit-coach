"""
Step 1 — PDF Extraction
Extracts raw workout text from monthly PDF files.
Output: raw-workouts.json structured by month -> week -> day.

Usage:
    python extract.py [--pdfs ../pdfs] [--output raw-workouts.json]
"""

import argparse
import json
import re
import sys
from pathlib import Path

import pdfplumber


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

DAY_PATTERNS = [
    re.compile(r'^\s*(monday|tuesday|wednesday|thursday|friday|saturday|sunday)\b', re.IGNORECASE),
    re.compile(r'^\s*day\s*[\d]+', re.IGNORECASE),
    re.compile(r'^\s*(mon|tue|wed|thu|fri|sat|sun)\b', re.IGNORECASE),
]

WEEK_PATTERNS = [
    re.compile(r'^\s*week\s*[\d]+', re.IGNORECASE),
    re.compile(r'^\s*w[\d]+\b', re.IGNORECASE),
]


def is_week_header(line: str) -> bool:
    return any(p.match(line) for p in WEEK_PATTERNS)


def is_day_header(line: str) -> bool:
    return any(p.match(line) for p in DAY_PATTERNS)


def normalise_month_name(path: Path) -> str:
    """Derive a human-readable month label from the filename."""
    stem = path.stem.lower()
    # Try to find a month name in the filename
    months = [
        'january', 'february', 'march', 'april', 'may', 'june',
        'july', 'august', 'september', 'october', 'november', 'december',
        'jan', 'feb', 'mar', 'apr', 'jun', 'jul', 'aug', 'sep', 'oct', 'nov', 'dec',
    ]
    for m in months:
        if m in stem:
            return stem
    return stem


# ---------------------------------------------------------------------------
# Core extraction
# ---------------------------------------------------------------------------

def extract_pdf(pdf_path: Path) -> dict:
    """Extract all text from a PDF and attempt month->week->day structuring."""
    month_label = normalise_month_name(pdf_path)
    result = {
        "file": pdf_path.name,
        "month": month_label,
        "raw_pages": [],
        "weeks": [],
    }

    all_lines: list[str] = []

    try:
        with pdfplumber.open(pdf_path) as pdf:
            for page_num, page in enumerate(pdf.pages, start=1):
                text = page.extract_text() or ""
                result["raw_pages"].append({
                    "page": page_num,
                    "text": text,
                })
                all_lines.extend(text.splitlines())
    except Exception as exc:
        result["error"] = str(exc)
        return result

    # Attempt to structure into weeks and days
    result["weeks"] = parse_weeks_and_days(all_lines)
    return result


def parse_weeks_and_days(lines: list[str]) -> list[dict]:
    """
    Best-effort parser: splits lines into weeks and days.
    Falls back to a single 'week 1' bucket if no week headers are found.
    """
    weeks: list[dict] = []
    current_week: dict | None = None
    current_day: dict | None = None

    def flush_day():
        if current_day and current_week is not None:
            # Strip trailing blank lines
            text = '\n'.join(current_day["lines"]).strip()
            current_day["text"] = text
            del current_day["lines"]
            current_week["days"].append(current_day)

    def flush_week():
        flush_day()
        if current_week is not None:
            weeks.append(current_week)

    for line in lines:
        stripped = line.strip()
        if not stripped:
            if current_day is not None:
                current_day["lines"].append("")
            continue

        if is_week_header(stripped):
            flush_week()
            current_week = {"week": stripped, "days": []}
            current_day = None
            continue

        if is_day_header(stripped):
            if current_week is None:
                current_week = {"week": "week 1", "days": []}
            flush_day()
            current_day = {"day": stripped, "lines": []}
            continue

        # Regular content line
        if current_day is not None:
            current_day["lines"].append(stripped)
        elif current_week is not None:
            # Content before first day header — attach to a preamble day
            current_day = {"day": "preamble", "lines": [stripped]}
        else:
            # Content before any week/day header
            current_week = {"week": "week 1", "days": []}
            current_day = {"day": "preamble", "lines": [stripped]}

    flush_week()
    return weeks


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description="Extract workout text from monthly PDFs.")
    parser.add_argument("--pdfs", default="../pdfs", help="Path to folder containing PDF files")
    parser.add_argument("--output", default="raw-workouts.json", help="Output JSON file path")
    args = parser.parse_args()

    pdfs_dir = Path(args.pdfs)
    if not pdfs_dir.exists():
        print(f"ERROR: PDFs folder not found: {pdfs_dir.resolve()}", file=sys.stderr)
        sys.exit(1)

    pdf_files = sorted(pdfs_dir.glob("*.pdf"))
    if not pdf_files:
        print(f"ERROR: No PDF files found in {pdfs_dir.resolve()}", file=sys.stderr)
        sys.exit(1)

    print(f"Found {len(pdf_files)} PDF(s) in {pdfs_dir.resolve()}")

    output: list[dict] = []
    for pdf_path in pdf_files:
        print(f"  Extracting: {pdf_path.name} ...", end=" ", flush=True)
        data = extract_pdf(pdf_path)
        week_count = len(data.get("weeks", []))
        day_count = sum(len(w["days"]) for w in data.get("weeks", []))
        if "error" in data:
            print(f"ERROR: {data['error']}")
        else:
            print(f"{week_count} week(s), {day_count} day entry(s)")
        output.append(data)

    out_path = Path(args.output)
    out_path.write_text(json.dumps(output, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"\nOutput written to: {out_path.resolve()}")
    print(f"Total months: {len(output)}")


if __name__ == "__main__":
    main()

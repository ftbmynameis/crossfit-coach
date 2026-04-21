# Step 1 — PDF Extraction

Extracts raw workout text from monthly PDF files and outputs `raw-workouts.json`.

## Setup

```bash
cd exercise-seed-generation/extract
pip install -r requirements.txt
```

## Usage

```bash
python extract.py
```

By default reads PDFs from `../pdfs/` and writes `raw-workouts.json` in the current directory.

### Options

```
--pdfs    Path to folder containing PDF files (default: ../pdfs)
--output  Output file path (default: raw-workouts.json)
```

## Output format

```json
[
  {
    "file": "january.pdf",
    "month": "january",
    "raw_pages": [
      { "page": 1, "text": "..." }
    ],
    "weeks": [
      {
        "week": "Week 1",
        "days": [
          { "day": "Monday", "text": "..." }
        ]
      }
    ]
  }
]
```

The parser makes a best-effort attempt to split content by week and day headers.
If the PDF layout doesn't match expected patterns, content falls back into a
`preamble` day entry — nothing is lost. The raw page text is always preserved
in `raw_pages` regardless of parsing success.

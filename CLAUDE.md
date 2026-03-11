# CLAUDE.md

## Project Overview
CrossFit Coach — a personal CrossFit workout tracker and weight suggestion app. The app allows a single user to log workouts, track performance over time, and receive weight/scaling suggestions before upcoming sessions. It acts as a personal CrossFit coach, optimizing suggestions to be challenging but completable.

## Tech Stack
- **Frontend:** Angular (latest stable), TypeScript strict mode, Angular Material
- **Backend:** ASP.NET Core (C#), REST API, Entity Framework Core
- **Database:** PostgreSQL hosted on Neon
- **ORM:** EF Core with code-first migrations
- **API Docs:** Swashbuckle (Swagger) — enabled in Development only
- **Auth:** Google OAuth2 (OIDC) — no local accounts, no password storage
- **Hosting:** Render (backend), Vercel (frontend)
- **CI/CD:** GitHub Actions
- **Repo:** Monorepo with `/client` and `/server` directories

## Architecture
- REST API backend — no GraphQL
- Feature-based folder structure on both client and server
- Code-first EF Core migrations — never edit the database directly
- Google OAuth2 flow: user authenticates with Google, backend validates the Google ID token and issues its own short-lived JWT for subsequent API calls
- Suggestion logic is split into two strategies:
  - `RuleBasedSuggestionService` — always available, uses past performance data
  - `LlmSuggestionService` — optional, calls external LLM API, gated behind a feature flag
- No microservices — keep it a single deployable backend service
- Angular frontend is a SPA — no SSR needed
- App must be fully usable on mobile browsers — design components mobile-first, responsive layout required throughout
- UI philosophy: **minimal by default, expandable on demand** — show only what's needed for the core action, hide optional complexity behind expand buttons or secondary actions. Never force the user to interact with fields they don't need.
- The workout input forms (Step 2 Prepare and Step 3 Log) are **optimised for phone use at the gym** — inputs must be large, thumb-friendly, and quick to fill in between sets or rounds. Minimal scrolling, minimal taps, no unnecessary fields visible by default. Assume the user is tired, in a hurry, and has one hand free.
- The `/exercise-seed-generation` toolchain is Python only — completely separate from the main stack, never imported or referenced by the web app

## Repository Structure
```
/
├── client/                        # Angular app
├── server/                        # ASP.NET Core API
│   ├── src/
│   └── tests/
├── exercise-seed-generation/      # Standalone Python toolchain — not part of the web app
│   ├── pdfs/                      # Input: 12 monthly workout PDFs (gitignored)
│   ├── extract/                   # Step 1: PDF extraction → raw-workouts.json
│   ├── review/                    # Step 2: Streamlit review UI → reviewed-workouts.json
│   └── generate/                  # Step 3: exercise list generation → exercises.json
├── .github/
│   └── workflows/                 # GitHub Actions CI/CD
├── CLAUDE.md
├── SPEC.md
├── TASKS.md
└── CHECKLIST.md
```

## Environments
| Environment | Where | Swagger | Notes |
|---|---|---|---|
| Development | Render (dev instance) | ✅ On | Points to Neon dev DB |
| Local | Developer machine | ✅ On | Points to Neon dev DB |
| Production | Render (later) | ❌ Off | Points to Neon prod DB |

- Set `ASPNETCORE_ENVIRONMENT=Development` in Render dev instance environment variables
- Never hardcode connection strings or secrets — always use environment variables
- All required environment variables must be documented in `.env.example` with descriptions

## Code Standards
- TypeScript strict mode, no `any`
- C# nullable reference types enabled
- All API endpoints must have Swagger XML doc comments
- All API request/response bodies must have dedicated DTO classes
- EF Core: no raw SQL unless absolutely necessary, use LINQ
- Validation: use Data Annotations or FluentValidation on all DTOs
- Errors: never swallow silently — always log or return structured error response
- Use `ProblemDetails` format for all API error responses

## Testing
- Backend only: xUnit for unit tests on service layer logic
- No frontend tests
- Focus testing on suggestion logic and workout similarity matching — these are the core algorithms
- No E2E tests at this stage

## CI/CD (GitHub Actions)
- On every PR: build backend (dotnet build + dotnet test) and build frontend (npm ci + ng build)
- On merge to `main`: auto-deploy backend to Render, auto-deploy frontend to Vercel
- PRs should be small and task-scoped
- **Claude Code must never push directly to `main` or merge its own PRs**
- All merges to `main` require explicit approval from the repo owner

## What Claude Should NOT Do
- Do not push directly to `main` or merge PRs — only the repo owner can do this
- Do not install new npm or NuGet packages without flagging it in a PR comment first
- Do not modify files outside the scope of the current task
- Do not use `any` in TypeScript
- Do not write migrations manually — always use `dotnet ef migrations add`
- Do not add LLM API calls to the rule-based suggestion path
- Do not create abstractions prematurely — keep it simple until complexity demands it
- Do not store secrets or API keys in code or committed files
- Do not skip DTO validation
- Do not create or modify GitHub branch protection rules

## Workflow
- **At the start of every session:** read `SPEC.md` in full before doing anything. It is the authoritative reference for what the app does, how the data model works, and why decisions were made. Never assume — check SPEC.md first.
- Each task from TASKS.md should be completed on a feature branch
- Branch naming: `feature/task-{id}-short-description` — the task number in the branch name is sufficient to track status
- Open a PR against `main` when the task is complete — do not merge it
- PR description must summarize what was done and how to test it
- As the final change in the PR, mark the task as ✅ in TASKS.md — no other status updates to TASKS.md

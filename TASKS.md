# TASKS.md
> Claude Code works through these tasks in order, one at a time, on a feature branch per task.
> 🧑 = requires user action before or during this task
> After Phase 1 is complete and verified, all subsequent tasks use the PR-based workflow.

## Status Legend
- 🔲 Not started
- 🔄 In progress (branch exists, work ongoing)
- 👀 PR open — awaiting your review and merge
- ✅ Merged to `main` (your call)
- ⏸ Blocked

---

## Phase 0: Exercise Seed Generation Toolchain
> Standalone Python toolchain — completely separate from the web app.
> Can be worked on independently and in parallel with Phase 1.

### TASK-P0-001 — PDF extraction script 🔲
**Goal:** Extract raw workout text from all 12 monthly PDFs without schema hints.
- Set up Python project in `/exercise-seed-generation/extract`
- Add `requirements.txt` with `pdfplumber` and dependencies
- Script accepts a `/pdfs` folder as input (gitignored — user places PDFs there)
- Extracts all text per page, attempts to identify month → week → day structure
- Makes no assumptions about layout consistency — extracts as much as possible gracefully
- Output: `raw-workouts.json` structured by month → week → day with raw text per entry
- Include a `README.md` explaining how to run

> 🧑 **User action:** Place all 12 monthly PDFs in `/exercise-seed-generation/pdfs/` before running

### TASK-P0-002 — Streamlit review UI 🔲
**Goal:** Interactive UI to review, correct, and approve extracted workout data.
- Set up Streamlit app in `/exercise-seed-generation/review`
- Loads `raw-workouts.json` from Step 1
- Displays workouts week by week, day by day in a readable format
- User can edit extracted text inline, flag issues, and mark entries as reviewed
- Saves output as `reviewed-workouts.json`
- Include a `README.md` explaining how to run (`streamlit run review.py`)

> 🧑 **User action:** Run the review UI, go through all extracted workouts, correct any parsing errors, save `reviewed-workouts.json`

**Depends on:** TASK-P0-001

### TASK-P0-003 — Exercise seed generation script 🔲
**Goal:** Derive a clean exercise list and generate `exercises.json` for DB import.
- Set up script in `/exercise-seed-generation/generate`
- Takes `reviewed-workouts.json` as input
- Extracts all unique exercise names mentioned across all workouts
- Cross-references against free-exercise-db (https://github.com/yuhonas/free-exercise-db)
- Maps to the app's category and muscle group system
- Outputs `exercises.json` ready for DB import
- Include a `README.md`

> 🧑 **User action:** Review `exercises.json` before importing — spot-check categories and muscle group assignments

**Depends on:** TASK-P0-002

---

## Phase 1: Initial Project Setup
> Goal: get a deployable "empty" project live on Render + Vercel with no auth, no features.
> Direct pushes to `main` are allowed during this phase only.
> PRs and branch protection are enabled AFTER this phase is verified working (see TASK-001-USER).

### TASK-001 — Repository & project scaffolding 👀
**Goal:** Monorepo with both projects initialised and building.
- Create monorepo structure with `/client`, `/server`, `/exercise-seed-generation` directories
- Scaffold ASP.NET Core Web API project in `/server`
- Scaffold Angular app in `/client` with Angular Material
- Add root files: `.gitignore`, `README.md`, `CLAUDE.md`, `SPEC.md`, `TASKS.md`, `CHECKLIST.md`
- Add `/exercise-seed-generation/pdfs/` to `.gitignore`
- Verify both projects build locally

> 🧑 **User action (before task):** Create the GitHub repository and provide the repo URL
> 🧑 **User action (after task):** Run both projects locally and confirm they build and run before continuing

### TASK-002 — Landing page (unauthenticated) ✅
**Goal:** A simple public landing page — placeholder for the future login page.
- Angular: basic landing page component at root route showing app name and a brief description
- No auth required — publicly accessible
- Backend: `GET /api/health` returning `200 OK` with app name and version
- Mobile-responsive layout using Angular Material

**Depends on:** TASK-001

### TASK-003 — GitHub Actions CI pipeline ✅
**Goal:** Automated build and test on every push, with manual trigger support.
- Create `.github/workflows/ci.yml`
- On every push and PR: build backend (`dotnet build` + `dotnet test`) and frontend (`npm ci` + `ng build`)
- Add `workflow_dispatch` trigger so the pipeline can be manually triggered from the GitHub Actions UI without needing a commit
- Both must pass

**Depends on:** TASK-002

### TASK-004 — Render backend deployment ✅
**Goal:** Backend auto-deploys to Render on push to `main` and is publicly accessible.
- Configure Render web service pointing at `/server` in the GitHub repo
- Add `render.yaml` or document build/start commands clearly in `README.md`
- Backend starts successfully and serves `GET /api/health`
- Swagger UI accessible at `https://{render-url}/swagger`

> 🧑 **User action (before task):** In Render dashboard, create web service, connect GitHub repo, set environment variables:
> - `ASPNETCORE_ENVIRONMENT=Development`
>
> That's all that's needed for now — JWT and Google OAuth credentials come later in Phase 2.
>
> 🧑 **User action (after task):** Confirm `https://{render-url}/api/health` returns 200 and Swagger loads. If deployment doesn't trigger automatically, go to GitHub → Actions → CI pipeline → "Run workflow" to trigger manually.

**Depends on:** TASK-003

### TASK-005 — Vercel frontend deployment ✅
**Goal:** Angular app auto-deploys to Vercel on push to `main` and is publicly accessible.
- Configure Vercel project pointing at `/client`
- Set `API_BASE_URL` environment variable on Vercel pointing to Render URL
- Confirm landing page loads at the Vercel URL on both mobile and desktop

> 🧑 **User action (before task):** Create Vercel project, connect GitHub repo, set `API_BASE_URL` to Render backend URL
> 🧑 **User action (after task):** Confirm landing page loads on both desktop and mobile. If deployment doesn't trigger automatically, use the Vercel dashboard to trigger a manual redeploy.

**Depends on:** TASK-003

### TASK-006 — EF Core + Neon database connection 🔲
**Goal:** Backend connects to Neon Postgres and initial migration runs successfully.
- Install EF Core + Npgsql provider in `/server`
- Configure connection string from `DATABASE_URL` environment variable
- Create initial empty migration
- Confirm migration runs and Neon connection is healthy
- Add `GET /api/health/db` endpoint confirming DB connectivity

> 🧑 **User action (before task):** Confirm Neon `crossfitcoach_dev` database exists and add `DATABASE_URL` to Render environment variables

**Depends on:** TASK-004

---

## Phase 2: Authentication & Permissions

### TASK-008 — Backend: Google OAuth2 + JWT + permissions 🔲
**Goal:** Users authenticate via Google. First login bootstraps superadmin.
- Implement Google OAuth2 OIDC flow in ASP.NET Core
- On successful Google login, validate ID token and issue a short-lived JWT (in-memory on client)
- Issue a **refresh token stored in an httpOnly cookie** — used to silently reissue JWT on page load, enabling persistent login across browser sessions without localStorage
- `POST /api/auth/refresh` — silently reissues JWT from refresh token cookie
- `POST /api/auth/logout` — clears refresh token cookie
- Granular permission system:
  - `exercises:read`, `exercises:create`, `exercises:edit`, `exercises:delete`
  - `workouts:read`, `workouts:create`, `workouts:edit`, `workouts:delete`
  - `logs:read`, `logs:create`, `logs:edit`, `logs:delete`
- No `superadmin` role — a user with all permissions is effectively the superadmin
- **First login bootstrap:** if authenticated email matches `ALLOWED_USER_EMAIL`, assign all permissions automatically
- Store user + permissions in DB
- All endpoints protected by `[Authorize]` unless explicitly public
- Swagger configured to accept Bearer token

> 🧑 **User action (before task):** In Google Cloud Console, add the Render backend URL to the authorised redirect URIs for your OAuth client (e.g. `https://{render-url}/api/auth/google/callback`). Also add `http://localhost:5000/api/auth/google/callback` for local development. Set `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`, `JWT_SECRET`, and `ALLOWED_USER_EMAIL` in Render environment variables.
> 🧑 **User action (after task):** Log in via Google and confirm superadmin permissions are assigned. Check via Swagger.

**Depends on:** TASK-001-USER

### TASK-009 — Frontend: Google login + authenticated home 🔲
**Goal:** Landing page becomes the login page. Authenticated users land on today's workout view.
- "Sign in with Google" button on landing page
- On successful login store JWT in memory (not localStorage)
- On page load: attempt silent token refresh via `POST /api/auth/refresh` — if successful, user is already logged in
- Redirect authenticated users to today's workout view (placeholder for now — shows today's date with options to Define / Prepare / Log depending on workout status)
- This screen is the long-term home of the app — eventually prefilled with the gym's workout
- Auth guard protecting all authenticated routes
- Unauthenticated users who fail the silent refresh see only the landing/login page

**Depends on:** TASK-008

---

## Phase 3: Data Model & Exercise Library

### TASK-010 — Analyse archived workouts & finalise data model 🔲
**Goal:** Derive the full data model from real workout data before building any CRUD.
- Analyse patterns from provided sample workouts: block types, exercise naming, rep schemes, measurement types, modifiers
- Validate the BlockDefinition / WorkoutDefinition / WorkoutSession / BlockSession model against real data
- Propose finalised schema for all entities including exercise modifiers, muscle group tagging, and default variants
- Define the fuzzy matching approach for BlockDefinition reuse — propose what parameters constitute "similar enough"
- Update `SPEC.md` with the finalised data model
- Validate: every sample workout must be representable in the proposed model

> 🧑 **User action (before task):** Provide 10–15 representative sample workouts from the archive (variety of block types, including any with modifiers like pause squats)
> 🧑 **User action (after task):** Review and approve the proposed data model and fuzzy match proposal — TASK-011 cannot start until approved
> ⏸ **Blocks:** TASK-011 and all subsequent tasks

**Depends on:** TASK-009

### TASK-011 — Backend: Full data model migration 🔲
**Goal:** All entities from the approved data model exist in the DB.
- Implement all EF Core entities per approved SPEC.md
- Generate and apply migration
- Confirm all tables created in Neon

**Depends on:** TASK-010

### TASK-012 — Backend: Exercise library API 🔲
**Goal:** CRUD for exercises respecting granular permissions.
- `GET /api/exercises` — list all (`exercises:read`)
- `GET /api/exercises/{id}` — single (`exercises:read`)
- `POST /api/exercises` — create (`exercises:create`)
- `PUT /api/exercises/{id}` — update name, muscle group, variants (`exercises:edit`)
- No single-exercise delete endpoint
- `POST /api/exercises/seed` — bulk import from `exercises.json` (`exercises:create`), fails if table not empty
- `DELETE /api/exercises/seed` — hard delete all exercises (`exercises:delete`)

**Depends on:** TASK-011

### TASK-013 — Frontend: Admin — exercise library 🔲
**Goal:** Admin section for managing exercises, with seed/erase controls gated by specific permissions.
- Admin link in navigation (hidden unless user has at least `exercises:edit`)
- List all exercises with category, muscle group, default variant and scaling variants (`exercises:read`)
- Add new exercise form (`exercises:create`)
- Edit exercise: name, muscle group, default variant, scaling variants (`exercises:edit`)
- **"Seed Exercises" button** — shown to users with `exercises:create`; imports `exercises.json`, disabled if table already has data
- **"Erase All Exercises" button** — shown only to users with `exercises:delete`; hard deletes all exercises; confirmation modal ("Are you sure? This cannot be undone.") with OK / Cancel before executing

> 🧑 **User action (after task):** Use the seed button in the admin UI to import `exercises.json` from TASK-P0-003

**Depends on:** TASK-012, TASK-P0-003

---

## Phase 4: Workout Definition (Step 1)

### TASK-014 — Backend: BlockDefinition, WorkoutDefinition & WorkoutSession APIs 🔲
**Goal:** Full API for creating and managing the three core workout entities.

**BlockDefinition endpoints:**
- `GET /api/blocks` — list all BlockDefinitions (`workouts:read`)
- `GET /api/blocks/{id}` — full detail with exercises (`workouts:read`)
- `POST /api/blocks` — create new BlockDefinition (`workouts:create`)
- `PUT /api/blocks/{id}` — update BlockDefinition (`workouts:edit`)
- `GET /api/blocks/{id}/sessions` — all BlockSessions for this definition, ordered by date (`workouts:read`) — used for progression tracking
- `GET /api/blocks/match` — fuzzy match endpoint: given a block structure, returns similar existing BlockDefinitions with similarity score (`workouts:read`)

**WorkoutDefinition endpoints:**
- `GET /api/workout-definitions` — paginated list (`workouts:read`)
- `GET /api/workout-definitions/{id}` — full detail with BlockDefinitions (`workouts:read`)
- `POST /api/workout-definitions` — create (`workouts:create`)
- `PUT /api/workout-definitions/{id}` — update (`workouts:edit`)

**WorkoutSession endpoints:**
- `GET /api/sessions` — paginated list, filterable by date (`workouts:read`)
- `GET /api/sessions/{id}` — full detail (`workouts:read`)
- `GET /api/sessions/date/{date}` — session for a specific date (`workouts:read`)
- `POST /api/sessions` — create a new session for a date, linked to a WorkoutDefinition (`workouts:create`)
- `PUT /api/sessions/{id}` — update session (`workouts:edit`)
- `DELETE /api/sessions/{id}` — delete session (`workouts:delete`)
- Session status: `Defined` → `Prepared` → `Logged`

**Depends on:** TASK-011

### TASK-015 — Frontend: Workout definition form (Step 1) 🔲
**Goal:** User can define today's workout, reusing existing BlockDefinitions where possible.
- User selects today's date — if a session already exists for that date, open it instead
- Default template pre-fills two blocks: strength + WOD
- Each block: type selector + dynamic exercise list
- Exercise selection: primary + variant autocomplete from library (variant optional, default implied)
- Optional modifier field per exercise (hidden by default, expandable — e.g. "3 sec pause")
- Strength block supports multiple exercises per round
- `for_time` supports both rep variation patterns (e.g. 21-15-9 and +2 burpees/round)
- **Fuzzy match prompt:** as each block is completed, app checks against existing BlockDefinitions. If a similar block is found, prompt: "This looks like [block name] — reuse it?" User can confirm reuse or create new.
- Save → WorkoutSession status: **Defined**
- Mobile-first layout

**Depends on:** TASK-014, TASK-013

---

## Phase 5: Workout Preparation (Step 2)

### TASK-016 — Backend: Block similarity engine 🔲
**Goal:** Find top 3 similar past BlockSessions for each block in today's session.
- `GET /api/blocks/{blockDefinitionId}/similar-sessions` — top 3 most similar past BlockSessions based on the fuzzy match approach defined in TASK-010
- Similarity is at the BlockDefinition level — not full workout level
- Returns similarity score + summary of what the user did (weights, results, RPE) in those sessions
- Also: `GET /api/sessions/{id}/similar` — convenience endpoint returning similar sessions per block for a full WorkoutSession

**Depends on:** TASK-014

### TASK-017 — Backend: Rule-based suggestion engine 🔲
**Goal:** Generate weight/scaling suggestions per block based on past BlockSession history.
- `POST /api/sessions/{id}/suggestions/rule-based` — accepts pre-workout feeling (1–5, optional)
- For each block in the session:
  - **Strength blocks:** looks at last 3–5 BlockSessions for the same BlockDefinition, tracks weight progression and RPE trend
  - **WOD blocks:** looks at last 3–5 BlockSessions for the same or similar BlockDefinition (per fuzzy match threshold from TASK-010)
  - Considers relationship between strength and WOD blocks (shared muscle groups, intensity context)
  - Applies modifier based on pre-workout feeling if provided
- Returns per-exercise: baseline suggestion + modifier + reasoning string
- Unit tests covering core suggestion logic for both strength and WOD block types

**Depends on:** TASK-016

### TASK-018 — Backend: Session preparation API 🔲
**Goal:** Save the user's preparation plan for a WorkoutSession.
- `POST /api/sessions/{id}/prepare` — save pre-workout feeling + planned weights/scaling per exercise
- `GET /api/sessions/{id}/prepare` — retrieve saved preparation plan
- Updates WorkoutSession status to **Prepared**

**Depends on:** TASK-014

### TASK-019 — Frontend: Workout preparation view (Step 2) 🔲
**Goal:** User builds their game plan before heading to the gym.
- Pre-workout feeling selector (1–5 smiley face, optional) — "How are you feeling today?"
- Per block:
  - "Suggest (Rule-based)" button — fields start empty, populated on press
  - Suggestion shows baseline + modifier with explanation if feeling is low
  - Top 3 similar past BlockSessions shown for reference
  - Per-exercise planned weight/scaling input (pre-fillable from suggestions)
  - Variant picker hidden by default, expandable via button next to weight input
  - Quick-add scaling variant inline (users with `exercises:edit` only)
- Save → WorkoutSession status: **Prepared**
- Mobile-first, optimised for quick input between sets

**Depends on:** TASK-017, TASK-018, TASK-013

---

## Phase 6: Workout Logging (Step 3)

### TASK-020 — Backend: Session log API 🔲
**Goal:** Record actual WorkoutSession results per block and exercise.
- `POST /api/sessions/{id}/log` — submit log entry (`logs:create`)
- `GET /api/sessions/{id}/log` — retrieve log (`logs:read`)
- `PUT /api/sessions/{id}/log` — update log (`logs:edit`)
- Stores per-BlockSession: per-exercise actuals (weight, reps/time/distance, variant), block result
- Stores overall RPE (1–5) and notes at WorkoutSession level
- Updates WorkoutSession status to **Logged**

**Depends on:** TASK-014

### TASK-021 — Frontend: Workout log form (Step 3) 🔲
**Goal:** User logs actual results after the session — optimised for gym use.
- Opens from a **Prepared** WorkoutSession
- Pre-workout feeling shown read-only for reference
- Per block, per exercise:
  - Weight input (large, thumb-friendly)
  - Reps / time / distance input depending on exercise type
  - Variant picker hidden by default, expandable via button
- Block result: total time / rounds / sets depending on block type
- Overall RPE (1–5 smiley face) — "How did that feel?"
- Notes field
- Save → WorkoutSession status: **Logged**
- Mobile-first, optimised for quick input between rounds

**Depends on:** TASK-020, TASK-019

---

## Phase 7: History & Overview

### TASK-022 — Frontend: Workout history view 🔲
**Goal:** Browsable history of all WorkoutSessions, with block-level progression visible.
- Scrollable list of WorkoutSessions, newest first
- Shows status per entry (Defined / Prepared / Logged)
- Filterable by: date range, exercise name, block type, muscle group
- Tap any session → full detail: WorkoutDefinition + preparation plan + logged BlockSessions
- **Block progression:** tapping a BlockDefinition name anywhere in the app shows all past BlockSessions for that block in chronological order — this is the core progression view for both strength and WOD blocks
- Mobile-first layout

**Depends on:** TASK-021

---

## Phase 8: LLM Suggestions (Optional / Gated)

### TASK-023 — Backend: LLM suggestion service 🔲
**Goal:** AI-powered suggestions via LLM API, gated behind feature flag.
- `POST /api/sessions/{id}/suggestions/llm` — requires credit, feature flag: `llm_suggestions_enabled`
- For each block: sends BlockDefinition + top 3 similar past BlockSessions + pre-workout feeling to LLM API
- Returns natural language suggestion with reasoning + feeling modifier per exercise
- Basic credit tracking in DB (1 credit per session suggestion call)

**Depends on:** TASK-017

### TASK-024 — Frontend: LLM suggestion UI 🔲
**Goal:** "Suggest (AI)" button per block in Step 2, alongside rule-based suggestions.
- Shown only if `llm_suggestions_enabled` feature flag is on
- Credit cost warning before calling
- AI suggestion displayed in visually distinct style from rule-based suggestion
- Rule-based and AI suggestions populate independently — user can use either or both

**Depends on:** TASK-023, TASK-019

---

## Backlog (Not Yet Scheduled)
- Personal records tracking per exercise and per BlockDefinition
- Progress charts (weight/performance over time per BlockDefinition — both strength and WOD)
- Production environment setup on Render (separate from dev instance)
- Push notifications / reminders before workout
- Export workout history to CSV
- Multi-user support with restricted permissions
- Coach-driven workout definition (Step 1 done by coach, not athlete)
- Handle new permissions added after initial bootstrap
- Direct search/reuse of existing WorkoutDefinitions when defining today's session

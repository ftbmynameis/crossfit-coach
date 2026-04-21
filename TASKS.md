# TASKS.md
> Claude Code works through these tasks in order, one at a time, on a feature branch per task.
> 🧑 = requires user action before or during this task

## Status Legend
- 🔲 Not started
- 🔄 In progress (branch exists, work ongoing)
- 👀 PR open — awaiting your review and merge
- ✅ Merged to `main` (your call)
- ⏸ Blocked

---

## Phase 1: Initial Project Setup
> Goal: get a deployable "empty" project live on Render + Vercel with no auth, no features.

### TASK-001 — Repository & project scaffolding ✅
### TASK-002 — Landing page (unauthenticated) ✅
### TASK-003 — GitHub Actions CI pipeline ✅
### TASK-004 — Render backend deployment ✅
### TASK-005 — Vercel frontend deployment ✅
### TASK-006 — EF Core + Neon database connection ✅

---

## Phase 2: Exercise Library
> Build the exercise library first — no auth required yet. Endpoints are open during this phase.
> Auth will be layered on in Phase 3.

### TASK-010 — Define Exercise data model 🔲
**Goal:** Agree on the Exercise schema before writing any code.
- Define the `Exercise` entity: name, category, muscle group, measurement type, is scalable
- Define the `ScalingVariant` entity: name, measurement type (may differ from default)
- Decide on category enum values based on the types seen in real workouts
- Decide on muscle group enum values
- Update `SPEC.md` with the finalised Exercise schema
- No code changes — this is a design task only

> 🧑 **User action:** Review and approve the proposed schema before TASK-011 begins

### TASK-011 — Backend: Exercise entity + migration 🔲
**Goal:** Exercise and ScalingVariant tables exist in the DB.
- Implement EF Core entities per approved SPEC.md schema
- Generate and run migration
- Confirm tables created in Neon

**Depends on:** TASK-010

### TASK-012 — Backend: Exercise library API 🔲
**Goal:** Full CRUD for exercises plus seeding from free-exercise-db.
- `GET /api/exercises` — list all, with scaling variants
- `GET /api/exercises/{id}` — single exercise with variants
- `POST /api/exercises` — create exercise
- `PUT /api/exercises/{id}` — update name, category, muscle group, measurement type, variants
- No single-exercise delete
- `POST /api/exercises/seed` — bulk import from free-exercise-db (https://github.com/yuhonas/free-exercise-db), mapped to app schema; fails if table not empty
- `DELETE /api/exercises/seed` — hard delete all exercises (for reseeding)
- All endpoints open (no auth) for now — auth will be added in Phase 3

**Depends on:** TASK-011

### TASK-013 — Frontend: Admin — exercise library 🔲
**Goal:** Admin page for viewing and managing exercises.
- Accessible at `/admin/exercises`
- Linked from main navigation
- List all exercises with category, muscle group, measurement type and scaling variants
- Add new exercise form
- Edit exercise: name, category, muscle group, measurement type, scaling variants
- **"Seed Exercises" button** — calls `POST /api/exercises/seed`; disabled if exercises already exist
- **"Erase All Exercises" button** — calls `DELETE /api/exercises/seed`; confirmation modal before executing
- No permission gating for now — auth added in Phase 3

**Depends on:** TASK-012

---

## Phase 3: Authentication & Permissions
> Wire up Google OAuth and apply permission checks to all existing endpoints.

### TASK-008 — Backend: Google OAuth2 + JWT + permissions 🔲
**Goal:** Users authenticate via Google. First login bootstraps full permissions.
- Implement Google OAuth2 OIDC flow in ASP.NET Core
- On successful Google login, validate ID token and issue a short-lived JWT (in-memory on client)
- Issue a **refresh token stored in an httpOnly cookie**
- `POST /api/auth/refresh` — silently reissues JWT from refresh token cookie
- `POST /api/auth/logout` — clears refresh token cookie
- Granular permission system:
  - `exercises:read`, `exercises:create`, `exercises:edit`, `exercises:delete`
  - `workouts:read`, `workouts:create`, `workouts:edit`, `workouts:delete`
  - `logs:read`, `logs:create`, `logs:edit`, `logs:delete`
- **First login bootstrap:** if authenticated email matches `ALLOWED_USER_EMAIL`, assign all permissions
- Store user + permissions in DB
- Apply `[Authorize]` to all existing exercise endpoints
- Swagger configured to accept Bearer token

> 🧑 **User action (before task):** Set up Google Cloud Console OAuth client, add redirect URIs for Render and localhost. Set `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`, `JWT_SECRET`, and `ALLOWED_USER_EMAIL` in Render environment variables.

**Depends on:** TASK-013

### TASK-009 — Frontend: Google login + authenticated routes 🔲
**Goal:** Landing page becomes login page. All app routes are auth-guarded.
- "Sign in with Google" button on landing page
- On successful login store JWT in memory (not localStorage)
- On page load: attempt silent token refresh via `POST /api/auth/refresh`
- Auth guard on all routes except landing page
- Admin section hidden unless user has `exercises:edit`
- Seed/erase buttons gated by `exercises:create` / `exercises:delete`

**Depends on:** TASK-008

---

## Phase 4: Workout Model
> Design and build the workout data model. Scope and complexity to be decided during design.
> BlockDefinition reuse and fuzzy matching are explicitly out of scope for now — each workout is standalone.

### TASK-014 — Design workout data model 🔲
**Goal:** Agree on the schema for WorkoutSession, Block, and BlockExercise before writing code.
- Define `WorkoutSession`: date, status (Defined/Prepared/Logged), pre-workout feeling, RPE, notes
- Define `Block`: type enum (strength/amrap/for_time/emom/chipper), order, time cap, result
- Define `BlockExercise`: exercise reference, order, prescribed parameters, modifier (free text)
- Define `ExerciseLog`: actual weight, reps/time/distance, variant used
- Update `SPEC.md` with finalised schema

> 🧑 **User action:** Review and approve schema before TASK-015 begins

**Depends on:** TASK-009

### TASK-015 — Backend: WorkoutSession API 🔲
**Goal:** API to create and manage workout sessions with blocks and exercises.
- `GET /api/sessions` — paginated list, filterable by date
- `GET /api/sessions/{id}` — full detail
- `GET /api/sessions/date/{date}` — session for a specific date
- `POST /api/sessions` — create session for a date
- `PUT /api/sessions/{id}` — update session
- `DELETE /api/sessions/{id}` — delete session
- Session status: `Defined` → `Prepared` → `Logged`

**Depends on:** TASK-014

### TASK-016 — Frontend: Workout definition form 🔲
**Goal:** User can define a workout for a given date.
- Select date → create new session or open existing
- Default two-block template (strength + WOD)
- Each block: type selector + exercise list (from library)
- Exercise selection with optional variant and optional modifier (hidden by default)
- Save → status: **Defined**
- Mobile-first layout

**Depends on:** TASK-015, TASK-013

---

## Phase 5: Workout Preparation & Logging

### TASK-017 — Backend: Preparation + logging APIs 🔲
**Goal:** Save pre-workout game plan and post-workout actuals.
- `POST /api/sessions/{id}/prepare` — save feeling + planned weights
- `GET /api/sessions/{id}/prepare` — retrieve plan → status: **Prepared**
- `POST /api/sessions/{id}/log` — save actual results
- `GET /api/sessions/{id}/log` — retrieve log → status: **Logged**
- `PUT /api/sessions/{id}/log` — update log

**Depends on:** TASK-015

### TASK-018 — Frontend: Preparation + log views 🔲
**Goal:** Step 2 (Prepare) and Step 3 (Log) UI.
- **Step 2:** pre-workout feeling selector, planned weights per exercise, save
- **Step 3:** actual results per exercise (weight + reps/time/distance), block result, RPE, notes, save
- Both optimised for mobile/phone use at the gym

**Depends on:** TASK-017, TASK-016

---

## Phase 6: History & Progression

### TASK-019 — Frontend: Workout history + exercise progression 🔲
**Goal:** Browse past sessions and track progression per exercise.
- Scrollable history of WorkoutSessions, newest first
- Filterable by date, exercise, block type
- Tap session → full detail
- Tap exercise name → all sessions containing that exercise in chronological order (progression view)

**Depends on:** TASK-018

---

## Backlog (Not Yet Scheduled)
- **Phase 0: PDF extraction toolchain** — extract exercises from gym's monthly programming PDFs (deferred; OCR complexity was too high for current value)
- BlockDefinition reuse + fuzzy matching — link recurring blocks across sessions for progression tracking
- Rule-based weight suggestion engine
- LLM-powered suggestion engine (gated behind feature flag)
- Personal records tracking per exercise
- Progress charts (weight/performance over time)
- Production environment setup on Render
- Push notifications / reminders
- Export workout history to CSV
- Multi-user support with restricted permissions
- Coach-driven workout definition

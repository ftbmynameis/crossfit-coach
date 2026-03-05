# SPEC.md
> This document describes **what** CrossFit Coach does and why. It is a stable reference — features are not removed when implemented. For task tracking see TASKS.md. The data model sections marked TBD will be updated after TASK-010 is completed.

## App Name
CrossFit Coach

## Purpose
A personal CrossFit workout tracker and AI-assisted coach for a single user. The app helps prepare for upcoming workouts by suggesting appropriate weights and scaling options, and allows logging of completed workouts to build a personal performance history.

## Users
- Single user (personal app) — authentication required for all features
- No broad "admin" role — permissions are granular:
  - `exercises:read`, `exercises:create`, `exercises:edit`, `exercises:delete`
  - `workouts:read`, `workouts:create`, `workouts:edit`, `workouts:delete`
  - `logs:read`, `logs:create`, `logs:edit`, `logs:delete`
- There is no `superadmin` role — a user with all permissions is effectively the superadmin
- `exercises:create` gates both adding individual exercises and seeding (bulk import)
- `exercises:delete` gates the erase all seeded data action (no single-exercise delete exists in the UI)
- The single user is granted all permissions on first login if their email matches `ALLOWED_USER_EMAIL`
- Future: may expand to small group of gym members with restricted permissions

---

## Core Concepts

### Workout Lifecycle
Every workout goes through three distinct steps:

1. **Define** — The workout structure is entered: blocks, exercises, prescribed sets/reps/time caps. Represents what the coach programmed. In the future this step may be performed by the coach directly, but for now the user does it.
2. **Prepare** — Before the session: rate pre-workout feeling, request suggestions, decide on personal scaling and target weights. This is the user's game plan.
3. **Log** — After the session: record actual reps completed, weights used, time, variants chosen, and post-workout RPE.

These three steps are clearly separated in the UI and data model. A workout can exist at any stage (defined but not prepared, prepared but not logged, fully logged).

### BlockDefinition
A reusable block template — the smallest reusable unit in the system. Describes a specific block of work: type, exercises, and prescribed parameters (sets, reps, time cap etc.). A BlockDefinition can be reused across many workout sessions over time, enabling progression tracking per block.

Examples:
- "5x5 Back Squat @ heavy"
- "12 min AMRAP: 10 pull-ups, 15 box jumps, 20 wall balls"
- "21-15-9 Thrusters + Pull-ups for time"

When a user defines a new block, the app performs a **fuzzy match** against existing BlockDefinitions (same exercises, similar parameters). If a match is found the user is prompted to confirm reuse. If not, a new BlockDefinition is created automatically. This prevents duplicates and enables progression tracking per block over time.

**Block types:**
| Type | Description | Example |
|---|---|---|
| `strength` | Sets x reps with rest periods, not for time | 5x5 Back Squat @ 80% |
| `amrap` | As many rounds as possible in a time cap | 12 min AMRAP: 10 pull-ups, 15 box jumps |
| `for_time` | Complete all work as fast as possible. Rep schemes can vary per round in two ways: fixed rep decrease/increase across rounds (e.g. 21-15-9), or a specific exercise scaling up/down each round (e.g. +2 burpees per round). Both patterns occur. | 3 rounds for time: 400m run, 21-15-9 KB swings |
| `emom` | Every minute on the minute | 10 min EMOM: odd = 5 cleans, even = 10 burpees |
| `chipper` | Long list of movements completed once | 50 wall balls, 40 box jumps, 30 pull-ups... |

A single WOD typically has 2 blocks: a **strength block** followed by a **WOD (HIT) block**. This two-block structure represents ~95% of workouts at this gym and should be the default template when entering a new workout.

### WorkoutDefinition
A named collection of BlockDefinitions representing a full training session as programmed. WorkoutDefinitions are reusable — the same combination of blocks may be programmed again months later.

### WorkoutSession
A specific execution of a WorkoutDefinition on a specific date. Contains the pre-workout feeling, planned weights (Step 2), and actual logged results (Step 3). Multiple WorkoutSessions can reference the same WorkoutDefinition, building a progression history over time.

### BlockSession
The logged results for a specific BlockDefinition within a WorkoutSession. Contains actual weights, reps, time, and variants. Contributes to the progression history of that BlockDefinition independently of the full workout.

### Exercise
A movement within a block (e.g. Back Squat, Pull-up, Row 500m).

Exercise properties — including how they're measured (weight, reps, distance, calories), whether they're scalable, and what common scaling variants exist — will be derived in **TASK-010** by analysing the 12-month archive of real gym workouts. The schema should not be assumed here.

Exercise categories:
- `barbell` — squat, deadlift, clean, snatch, etc.
- `gymnastics` — pull-ups, handstands, muscle-ups, etc.
- `cardio_machine` — rower, bike, ski erg
- `kettlebell_dumbbell`
- `bodyweight` — burpees, box jumps, etc.
- `run` — measured by distance or time
- `jump_rope` — primary exercise is Jumping Rope; standard variant is double unders (measured by reps); scaling variants are single unders and double under attempts (measured by time)

**Exercise + Variant model:** Exercises follow a primary + variant pattern. The primary exercise is the movement (e.g. Squat) and the variant specifies the form or scaling (e.g. Back, Front, Bodyweight). Each exercise has a **default variant** (e.g. Squat → Back Squat, Jumping Rope → Double Unders). The variant field is **optional** in both workout definition and logging — if left empty it implies the default variant. This keeps input fast for standard cases and handles mixed-variant workouts (e.g. varying variants across rounds) without forcing a choice. Scaling variants may have a different measurement type than the default variant (e.g. double unders by reps, single unders by time). Exact exercise schema to be derived in TASK-010.

**Exercise modifiers:** Some exercises can have additional context-specific parameters that don't fit neatly into the variant model. For example, a pause squat (holding the bottom position for 1–3 seconds) is still a Back Squat but with a meaningfully different stimulus. Rather than enumerating all possible modifiers per exercise, a free-text `modifier` field at the BlockExercise level handles this generically (e.g. "3 sec pause at bottom", "tempo 3-1-3"). This keeps the model flexible without over-engineering it.

**Muscle group tagging:** Each exercise is tagged with a primary muscle group or focus. Examples: `legs`, `back`, `shoulders`, `core`, `full_body`, `cardio`, `hit`. This tagging rolls up to:
- **Block level** — each BlockDefinition is categorised by the muscle groups it targets
- **Workout level** — overall WorkoutDefinition is categorised by combined muscle group focus
- Used by the suggestion engine to identify structurally similar blocks and understand the relationship between strength and WOD blocks

### WorkoutSession Log
A completed WorkoutSession record, containing:
- Date performed
- Pre-workout feeling (1–5, smiley face — optional, logged for reference and used in suggestions)
- For each BlockSession:
  - For each exercise: weight used, reps/time/distance completed, variant chosen
  - Block result: time completed, rounds completed, or sets completed
- Overall RPE (rate of perceived exertion, **1–5 scale**)
- Notes (free text)

> **UI Note (for logging task):** Both pre-workout feeling and RPE use the same smiley face selector (1 = very sad, 3 = neutral, 5 = very happy). Labels differ: pre-workout = "How are you feeling today?", post-workout = "How did that feel?"

---

## Features

### 1. Step 1 — Workout Definition
Workout input is split into three distinct steps: defining, preparing, and logging. This section covers definition only.

- User selects today's date and builds a WorkoutDefinition using BlockDefinitions
- Default template pre-fills two blocks: a strength block and a WOD block (~95% of workouts)
- Each block has a type selector and a dynamic exercise list
- Exercises selected from the pre-seeded library using primary + variant pattern
- A strength block can contain multiple exercises per round (e.g. Back Squat + L-sit)
- Exercises cannot be added inline — the exercise library is admin-managed only
- **Fuzzy match on block entry:** as the user defines each block, the app checks against existing BlockDefinitions. If a similar block is found (same exercises, similar parameters) the user is prompted: "This looks like [existing block name] — reuse it?" Confirming reuse links this session to the existing BlockDefinition. Declining creates a new one.
- WorkoutSession created for today's date, linked to the WorkoutDefinition, with status: **Defined**
- Future: coach may define workouts directly, removing this step for the athlete

### 2. Step 2 — Prepare (Scaling & Weight Plan)
- User opens a defined workout and sets their personal game plan before heading to the gym
- Rate pre-workout feeling (1–5 smiley face) — optional but influences suggestions
- Suggestion fields start **empty** — user triggers them explicitly:
  - **"Suggest (Rule-based)"** — always available, populates baseline + modifier per exercise
  - **"Suggest (AI)"** — gated, populates a separate AI suggestion field
- Each suggestion shows a **baseline** recommendation plus a **modifier** if pre-workout feeling is low, e.g.:
  > Back Squat: 80kg *(−5kg — energy rated 2/5 today)*
- User can search top 3 similar past workouts for manual reference
- User enters their **planned scaling and weights** per exercise based on suggestions + similar workouts
- Workout status updates to: **Prepared**
- The suggestion engine analyses the **entire block as the unit of comparison**, not individual exercises:
  - **Strength blocks** — typically single exercise, so block-level and exercise-level comparison converge naturally. Weight progression is tracked through recurring BlockDefinitions (e.g. "5x5 Back Squat" across sessions).
  - **WOD/HIT blocks** — the full block is the meaningful unit. Comparing push-ups within a "21-15-9 Thrusters + Pull-ups" block to push-ups in an unrelated 10-exercise chipper is not meaningful. Only sessions sharing the same or very similar BlockDefinition are used for WOD exercise suggestions.
  - **Open question:** the exact threshold for what makes two blocks "similar enough" to compare progression across is not yet defined. This should be determined during TASK-010 when real workout data is available to reason about.
- **Rule-based strategy:**
  - Analyses full workout structure, looks at last 3–5 structurally similar past workouts
  - Adjusts baseline based on RPE trend + pre-workout feeling modifier
  - Returns suggestion + reasoning string per exercise
- **LLM strategy (gated):**
  - Sends full workout + similar workout logs + pre-workout feeling to LLM API
  - Returns natural language suggestion with reasoning and feeling-based modifier
  - Costs 1 credit per use — feature flag: `llm_suggestions_enabled`

### 3. Step 3 — Log (Actual Results)
- User opens a prepared workout after completing the session
- Pre-workout feeling already saved from Step 2 — shown for reference
- For each block, actual results are entered:
  - Per exercise: weight used, reps/time/distance completed, variant chosen
  - Block result: total time, rounds completed, or sets completed
- Overall RPE (1–5, smiley face)
- Notes field (free text)
- Workout status updates to: **Logged**
- Saved log feeds future suggestions

### 4. Workout History
- Scrollable list of all logged workouts, newest first
- Filterable by: date range, exercise name, block type
- Tapping a past workout shows full definition + logged results

### 5. Exercise Library (requires `exercises:read` or higher)
- Accessible via a dedicated Admin section — only shown to users with at least `exercises:edit`
- Users with `exercises:edit` can: add exercises (requires `exercises:create`), edit name/muscle group/scaling variants
- Exercises cannot be deleted individually — no single-exercise delete exists
- **Shown only to users with `exercises:delete`:**
  - **Seed Exercises** button — imports `exercises.json` into DB, only if exercise table is empty (requires `exercises:create`)
  - **Erase All Exercises** button — hard deletes all exercises (requires `exercises:delete`); confirmation modal (OK / Cancel) before executing
- Quick-add scaling variant during Step 2 (Prepare) for users with `exercises:edit`

### 6. Exercise Library Seeding (External — not part of the web app)
The exercise library is seeded via a **standalone Python toolchain** in a dedicated `/exercise-seed-generation` folder in the repo. It runs once when the exercise table is empty and is never run again. Workout history is **not imported** — past workouts were defined by the coach and will be entered manually via the app's workout definition flow.

**Toolchain language: Python** (uses `pdfplumber` for extraction, `Streamlit` for the review UI)

**Three-step process:**

**Step 1 — PDF Extraction (`/exercise-seed-generation/extract`)**
- Input: one PDF per month (12 PDFs total), placed in a `/pdfs` subfolder
- Extracts everything it can from each PDF without schema hints — full workout text per day, per week
- Output: `raw-workouts.json` containing all extracted text structured by month → week → day
- No attempt to interpret or clean at this stage — raw extraction only
- Inconsistent layouts and naming are expected and handled gracefully (partial extraction is fine)

**Step 2 — Streamlit Review UI (`/exercise-seed-generation/review`)**
- Spins up a local interactive UI: `streamlit run review.py`
- Displays extracted workouts week by week, day by day
- User can see exactly what was parsed, spot where the extractor got confused, edit inline, and flag issues
- Saves reviewed/corrected output as `reviewed-workouts.json`
- This is the manual cleanup step before generating the final exercise list

**Step 3 — Seed Generation (`/exercise-seed-generation/generate`)**
- Takes `reviewed-workouts.json` from Step 2
- Derives a clean exercise list from all movements mentioned across all workouts
- Cross-references against free-exercise-db to enrich with metadata
- Maps to the app's category and muscle group system
- Outputs final `exercises.json` ready for import into the DB

**Step 4 — Import**
- A simple API endpoint reads `exercises.json` and inserts into the DB
- Only runs if the exercise table is empty — fails with a clear error otherwise
- Requires `exercises:create` permission to call

---

## Data Model (Draft — to be finalised in TASK-010)

> The full schema will be derived by analysing the 12-month workout archive in TASK-010. SPEC.md will be updated with the finalised model before TASK-011 begins. The below is a structural outline only.

```
User
- Id, Email (Google), GoogleSubjectId, CreatedAt
  (no password — Google OAuth only)

Exercise
- Id, Name, PrimaryMovement, DefaultVariant, Category, MuscleGroup
- MeasurementType, IsScalable
- ScalingVariants: [] (schema TBD in TASK-010)

BlockDefinition  (reusable block template)
- Id, Name (optional), Type (enum), TimeCap, Notes, CreatedAt
- MuscleGroups: derived from exercises
- BlockExercises: BlockExercise[]

BlockExercise
- Id, BlockDefinitionId, ExerciseId, Order
- Modifier (free text, optional — e.g. "3 sec pause", "tempo 3-1-3")
- Prescribed parameters (schema TBD in TASK-010 — varies by block type and exercise)

WorkoutDefinition  (reusable full session template)
- Id, Title (optional), Notes, CreatedAt
- MuscleGroups: derived from blocks
- Blocks: BlockDefinition[]

WorkoutSession  (a specific execution on a specific date)
- Id, WorkoutDefinitionId, DatePerformed
- PreWorkoutFeeling (1–5, optional)
- PlannedWeights (Step 2 preparation data — schema TBD)
- OverallRpe (1–5), Notes, CreatedAt
- Status: Defined | Prepared | Logged
- BlockSessions: BlockSession[]

BlockSession  (logged results for one block within a session)
- Id, WorkoutSessionId, BlockDefinitionId
- Result (flexible — time/rounds/sets, schema TBD in TASK-010)
- ExerciseLogs: ExerciseLog[]

ExerciseLog
- Id, BlockSessionId, BlockExerciseId
- Actual performance values (schema TBD in TASK-010)
- Variant chosen (default implied if empty)
- Notes
```

---

## User Flows

### Flow 1: Step 1 — Define today's workout
1. Open app → tap "New Workout" for today's date
2. Default two-block template pre-fills: strength block + WOD block
3. Fill in each block: type, exercises (primary + variant, variant optional), prescribed reps/sets/time cap
4. Optionally expand the modifier field per exercise (e.g. "3 sec pause")
5. When each block is complete, app checks for similar existing BlockDefinitions — if found, prompts "This looks like [name] — reuse it?" Confirm to reuse, decline to create new
6. Save → WorkoutSession status: **Defined**

### Flow 2: Step 2 — Prepare (before heading to the gym)
1. Open today's defined WorkoutSession
2. Rate pre-workout feeling (1–5 smiley face) — optional
3. Per block: tap "Suggest (Rule-based)" to populate suggestion fields (start empty)
4. Each suggestion shows baseline weight/scaling + modifier if feeling was rated low
5. Top 3 similar past BlockSessions shown per block for manual reference
6. Enter planned scaling and weights per exercise (variant picker hidden by default — expand if needed)
7. Save → WorkoutSession status: **Prepared**

### Flow 3: Step 3 — Log results (after the session)
1. Open today's prepared WorkoutSession
2. Pre-workout feeling shown read-only for reference
3. Per block, per exercise: enter actual weight (large input), reps/time/distance; expand variant picker only if needed
4. Enter block result (time / rounds / sets depending on type)
5. Select overall RPE (1–5 smiley face) — "How did that feel?"
6. Add optional notes
7. Save → WorkoutSession status: **Logged**

### Flow 4: Review history
1. Open "History" tab
2. Browse or filter past WorkoutSessions by date, exercise, block type, or muscle group
3. Tap any session → full detail: WorkoutDefinition + preparation plan + logged BlockSessions
4. Tap any BlockDefinition name → progression view showing all past BlockSessions for that block in chronological order

### Flow 5: Manage exercise library (admin)
1. Open Admin section (visible to users with `exercises:edit`)
2. Add or edit exercises, set default variant, scaling variants, muscle group, and measurement type
3. Seed exercises from `exercises.json` (requires `exercises:create`)
4. Erase all exercises if reseeding is needed (requires `exercises:delete` — confirmation modal)

---

## Out of Scope (for now)
- Multiple users / social features
- Gym programming (app consumes workouts, doesn't create programming)
- Nutrition tracking
- Video/form coaching
- Mobile app (responsive web is the target — native iOS/Android is out of scope)
- Automatic workout import from gym's website/app

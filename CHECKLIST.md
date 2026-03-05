# CHECKLIST.md
> Manual tasks for the repo owner. These are things Claude Code cannot do for you.
> Work through Phase 0 before running any Claude Code tasks.

---

## Phase 0: Account Setup & External Services

### Accounts to create (if you don't have them already)
- [ ] **GitHub** — https://github.com — create account + new repository (monorepo, public or private)
- [ ] **Neon** — https://neon.tech — create account + new Postgres project, create two databases: `crossfitcoach_dev` and `crossfitcoach_prod`
- [ ] **Render** — https://render.com — create account
- [ ] **Vercel** — https://vercel.com — create account
- [ ] **Google Cloud Console** — https://console.cloud.google.com — create account (needed for Google OAuth)

---

## Phase 1: GitHub Repository Setup

- [ ] Create a new GitHub repository (suggested name: `crossfit-coach`)
- [ ] Set default branch to `main`
- [ ] Enable branch protection on `main`:
  - Go to **Settings → Branches → Add branch protection rule**
  - Branch name pattern: `main`
  - ✅ Require a pull request before merging
  - ✅ Require approvals (set to 1)
  - ✅ Do not allow bypassing the above settings
- [ ] Clone the repo locally and add the three spec files (`CLAUDE.md`, `SPEC.md`, `TASKS.md`, `CHECKLIST.md`) to the root, then push

---

## Phase 2: Google OAuth Setup

- [ ] Go to https://console.cloud.google.com
- [ ] Create a new project (e.g. `crossfit-coach`)
- [ ] Navigate to **APIs & Services → OAuth consent screen**
  - User type: External
  - Fill in app name, support email
  - Add your own Google account as a test user
- [ ] Navigate to **APIs & Services → Credentials → Create Credentials → OAuth 2.0 Client ID**
  - Application type: Web application
  - Authorised redirect URIs — add both:
    - `https://{your-render-url}/api/auth/google/callback`
    - `http://localhost:5000/api/auth/google/callback`
- [ ] Copy and save: **Client ID** and **Client Secret**
- [ ] Add the following to Render environment variables (needed before TASK-008):
  - [ ] `GOOGLE_CLIENT_ID`
  - [ ] `GOOGLE_CLIENT_SECRET`
  - [ ] `JWT_SECRET` (generate a random 64-char string)
  - [ ] `ALLOWED_USER_EMAIL` (your Google account email)

---

## Phase 3: Neon Database

- [ ] Create project in Neon dashboard
- [ ] Create two databases: `crossfitcoach_dev` and `crossfitcoach_prod`
- [ ] Copy connection strings for both (you'll need these for Render environment variables)
- [ ] Save connection strings somewhere safe (password manager recommended)

---

## Phase 4: Render Setup

- [ ] Create a new **Web Service** on Render
  - Connect your GitHub repository
  - Root directory: `server`
  - Build command: `dotnet publish -c Release -o out`
  - Start command: `dotnet out/{AppName}.dll`
- [ ] Set environment variables on the Render dev service:
  - [ ] `ASPNETCORE_ENVIRONMENT` = `Development`
  - [ ] `DATABASE_URL` = *(Neon dev connection string — add this when TASK-006 begins)*
  - [ ] `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`, `JWT_SECRET`, `ALLOWED_USER_EMAIL` — *(add these when TASK-008 begins, see Phase 2: Google OAuth Setup above)*
- [ ] Confirm service deploys successfully and Swagger is accessible at `https://{render-url}/swagger`

---

## Phase 5: Vercel Setup

- [ ] Create a new project on Vercel
  - Connect your GitHub repository
  - Root directory: `client`
  - Framework preset: Angular
- [ ] Set environment variables on Vercel:
  - [ ] `API_BASE_URL` = *(your Render backend URL)*
- [ ] Confirm Angular app deploys and loads at the Vercel URL

---

## Phase 6: Local Development Setup

- [ ] Install required tools locally:
  - [ ] Node.js (LTS)
  - [ ] .NET SDK (latest stable)
  - [ ] Angular CLI (`npm install -g @angular/cli`)
  - [ ] EF Core CLI (`dotnet tool install --global dotnet-ef`)
  - [ ] Claude Code (`npm install -g @anthropic-ai/claude-code`)
- [ ] Create `server/.env` locally with the same variables as Render (point `DATABASE_URL` to Neon dev DB)
- [ ] Confirm `server/.env` is in `.gitignore` — never commit this file

---

## Ongoing: Per-Task Checklist
> Some Claude Code tasks will require you to take action before or after.

- [ ] **After TASK-001:** Review scaffolded project structure, confirm it looks right before pushing to `main`
- [ ] **Before TASK-006:** Add `DATABASE_URL` to Render environment variables (Neon dev connection string)
- [ ] **Before TASK-008:** Add `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`, `JWT_SECRET`, `ALLOWED_USER_EMAIL` to Render environment variables; add redirect URIs in Google Cloud Console
- [ ] **Before TASK-010:** Provide Claude Code with 10–15 sample workouts from your archive (variety of block types, including any with modifiers like pause squats)
- [ ] **After TASK-010:** Review and approve the proposed data model before TASK-011 begins
- [ ] **After TASK-013:** Use the Seed Exercises button in the admin UI to import `exercises.json`
- [ ] **Each PR:** Review, test if needed, approve and merge to `main`

# CrossFit Coach

A personal CrossFit workout tracker and weight suggestion app. Log workouts, track performance over time, and get weight/scaling suggestions before each session.

## Structure

```
/
├── client/                    # Angular frontend (SPA)
├── server/                    # ASP.NET Core backend (REST API)
│   ├── src/CrossfitCoach.Api/ # Web API project
│   └── tests/CrossfitCoach.Tests/
├── exercise-seed-generation/  # Standalone Python toolchain (see below)
├── SPEC.md                    # Full app specification
├── TASKS.md                   # Development task list
└── CHECKLIST.md               # Manual setup checklist
```

## Tech Stack

- **Frontend:** Angular + Angular Material
- **Backend:** ASP.NET Core 9, C#
- **Database:** PostgreSQL on Neon (via EF Core)
- **Auth:** Google OAuth2 → JWT
- **Hosting:** Render (backend), Vercel (frontend)

## Running Locally

### Backend

Requires [.NET 9 SDK](https://dotnet.microsoft.com/download).

```bash
cd server
dotnet restore
dotnet run --project src/CrossfitCoach.Api
# Swagger UI (HTTP):  http://localhost:5000/swagger
# Swagger UI (HTTPS): https://localhost:5001/swagger  (requires trusted dev cert)
```

> If HTTPS fails in the browser, run `dotnet dev-certs https --trust` once, then retry.
> Or use the HTTP URL — both work locally.

Run tests:

```bash
cd server
dotnet test
```

### Frontend

Requires [Node.js 20+](https://nodejs.org) and Angular CLI.

```bash
cd client
npm install
ng serve
# App: http://localhost:4200
```

## Environment Variables

Copy `.env.example` (created in a later task) and fill in values. Never commit secrets.

| Variable | Description |
|---|---|
| `DATABASE_URL` | Neon PostgreSQL connection string |
| `GOOGLE_CLIENT_ID` | Google OAuth2 client ID |
| `GOOGLE_CLIENT_SECRET` | Google OAuth2 client secret |
| `JWT_SECRET` | Secret key for signing JWTs |
| `ALLOWED_USER_EMAIL` | Email that receives all permissions on first login |

## Exercise Seed Toolchain

Standalone Python toolchain in `/exercise-seed-generation` — completely separate from the web app. See individual `README.md` files in each subdirectory for usage.

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

## Deployment

### Backend — Render

The `render.yaml` at the repo root configures the Render web service automatically when you connect the repo via the Render Blueprint flow.

**Manual dashboard setup (alternative):**
| Field | Value |
|---|---|
| Build command | `dotnet publish server/src/CrossfitCoach.Api/CrossfitCoach.Api.csproj -c Release -o publish` |
| Start command | `./publish/CrossfitCoach.Api` |
| Environment | Set `ASPNETCORE_ENVIRONMENT=Development` |
| Port binding | Set `ASPNETCORE_URLS=http://+:$PORT` |

Once deployed, verify:
- `GET https://{render-url}/api/health` → `200 OK`
- `https://{render-url}/swagger` → Swagger UI (Development only)

### Frontend — Vercel

_(Configured in TASK-005)_

## Environment Variables

Copy `.env.example` and fill in your values. Never commit secrets.

| Variable | Description |
|---|---|
| `DATABASE_URL` | Neon PostgreSQL connection string |
| `GOOGLE_CLIENT_ID` | Google OAuth2 client ID |
| `GOOGLE_CLIENT_SECRET` | Google OAuth2 client secret |
| `JWT_SECRET` | Secret key for signing JWTs — generate with `openssl rand -base64 64` |
| `ALLOWED_USER_EMAIL` | Email that receives all permissions on first login |
| `ASPNETCORE_ENVIRONMENT` | Set to `Development` to enable Swagger and dev features |
| `ASPNETCORE_URLS` | Binding address — use `http://+:$PORT` on Render |

## Exercise Seed Toolchain

Standalone Python toolchain in `/exercise-seed-generation` — completely separate from the web app. See individual `README.md` files in each subdirectory for usage.

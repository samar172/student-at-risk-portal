# Student At-Risk Early Warning and Counselling Portal

A data-driven web application that analyses student **attendance, marks and participation** to identify
at-risk students and alert counsellors for early intervention. Inspired by SIH-1661 (*Reduce Student Dropout Rates*).

Built with **ASP.NET Core MVC (.NET 10)**, **SQL Server (EF Core Code First)**, Razor views and a RESTful API for data ingestion.

The full specification is in [`docs/`](docs/) (Software Requirements Specification).

## Status

| Phase | Scope | State |
|---|---|---|
| 0 | Solution skeleton, layered projects, CI | Done |
| 1 | Domain model, EF Core schema + initial migration, Identity (4 roles), login/logout | Done |
| 1 | Risk calculator (SRS 4.5) with unit tests | Done |
| 2 | Master data CRUD, CSV import | Next |
| 3 | Ingestion REST API (API key, bulk upsert, Swagger) | Planned |
| 4 | Nightly scoring job, risk snapshots, alerts | Planned |
| 5 | Dashboards, Student 360, charts | Planned |
| 6 | Counsellor notes, interventions | Planned |
| 7 | Notifications (outbox, e-mail, escalation) | Planned |

## Solution layout

```
src/
  Portal.Domain          entities and enums (no dependencies)
  Portal.Application     risk calculator and (soon) services, DTOs, validators
  Portal.Infrastructure  EF Core DbContext, migrations, Identity, seeding
  Portal.Web             MVC controllers, Razor views, API controllers
tests/
  Portal.UnitTests       xUnit tests (risk rules, boundaries, worked example)
docs/                    SRS report
```

## Getting started

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download), Docker (for local SQL Server).

```bash
# 1. Start SQL Server
cp .env.example .env            # set a strong MSSQL_SA_PASSWORD
docker compose up -d

# 2. Configure the app (kept out of source control via user-secrets)
cd src/Portal.Web
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=StudentRiskPortal;User Id=sa;Password=<your MSSQL_SA_PASSWORD>;TrustServerCertificate=True"
dotnet user-secrets set "Seed:AdminEmail" "admin@example.com"
dotnet user-secrets set "Seed:AdminPassword" "<a strong password, 10+ chars with a symbol>"

# 3. Run (Development applies migrations, creates roles and the admin user on startup)
dotnet run --launch-profile https
```

Then open the URL shown in the console and sign in with the seeded administrator. `GET /health` reports app and database status.

### Tests

```bash
dotnet test
```

### Database migrations

```bash
dotnet tool restore
dotnet ef migrations add <Name> -p src/Portal.Infrastructure -s src/Portal.Web -o Persistence/Migrations
```

## Security notes

- No credentials are committed. The connection string in `appsettings.json` is a placeholder; real values come from user-secrets or environment variables.
- Passwords: minimum 10 characters with a symbol; accounts lock for 15 minutes after 5 failed sign-ins.
- Anti-forgery validation is enforced globally on state-changing requests; login redirects accept local URLs only.

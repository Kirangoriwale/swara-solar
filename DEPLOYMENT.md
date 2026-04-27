# SolarBilling Deployment Guide

## 1) Supabase PostgreSQL setup

1. Create a Supabase project.
2. Open `Project Settings -> Database`.
3. Copy the connection string and keep SSL enabled.
4. Set the connection string in your environment:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=...supabase.co;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require;Trust Server Certificate=true"
```

## 2) Apply EF Core migrations

```powershell
dotnet tool restore
dotnet ef database update
```

## 3) Render deployment from GitHub

1. Push this project to GitHub.
2. In Render, create a new **Web Service** from the GitHub repo.
3. Keep the root directory at the project root (where `render.yaml` exists).
4. Add environment variable:
   - `ConnectionStrings__DefaultConnection` = your Supabase PostgreSQL connection string
5. Deploy.

Render uses:
- build: `dotnet restore && dotnet publish -c Release -o publish`
- start: `dotnet ./publish/SolarBilling.dll`

Database migrations are applied by the app on startup via EF Core `Database.Migrate()`.

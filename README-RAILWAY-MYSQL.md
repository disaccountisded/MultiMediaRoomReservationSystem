# Multi Media Room Reservation System — Railway/MySQL configuration

This package preserves the original repository's Models, Controllers, Services, Razor Views, Bootstrap UI, and reservation rules. The deployment changes are limited to database configuration, Railway port binding, Docker build files, and migration instructions.

## Technology
- Visual Studio 2022
- ASP.NET Core MVC / .NET 9
- Entity Framework Core 9
- Pomelo.EntityFrameworkCore.MySql 9
- MySQL locally and on Railway
- Bootstrap 5.3.3

## A. Local development with Visual Studio 2022

1. Install the .NET 9 SDK and MySQL Server.
2. Open `MultiMediaRoomReservationSystem.csproj` in Visual Studio 2022.
3. Start your local MySQL server.
4. Open `appsettings.json`.
5. Replace `YOUR_LOCAL_MYSQL_USER` and `YOUR_LOCAL_MYSQL_PASSWORD` with your local MySQL credentials. Do not commit real credentials to a public GitHub repository.
6. Create the database if your MySQL account does not have permission to create databases:

```sql
CREATE DATABASE MultiMediaRoomReservationDB;
```

7. In Visual Studio, open Tools > NuGet Package Manager > Package Manager Console.
8. Run the migration commands in section C below.
9. Run the project. Your original launch profile remains `https://localhost:51100` / `http://localhost:51101`.

## B. Railway variables

In the **MultiMediaRoomReservationSystem application service**, make sure these Railway references are available from your MySQL service:

- `MYSQLHOST`
- `MYSQLPORT`
- `MYSQLDATABASE`
- `MYSQLUSER`
- `MYSQLPASSWORD`

You may also set one combined variable in the application service:

Variable name:
`ConnectionStrings__DefaultConnection`

Variable value:
`Server=${{MYSQLHOST}};Port=${{MYSQLPORT}};Database=${{MYSQLDATABASE}};User=${{MYSQLUSER}};Password=${{MYSQLPASSWORD}};`

Use the private/internal `MYSQLHOST` and `MYSQLPORT` references. Do not use `MYSQL_PUBLIC_URL` for service-to-service traffic. Do not paste real passwords into source code or GitHub.

The updated `Program.cs` uses the combined connection string if supplied. In Production, if the combined variable is not supplied, it builds the connection string from the five `MYSQL*` variables.

Railway's `PORT` variable is read automatically. Kestrel binds to `0.0.0.0:$PORT`. Do not set the website to a hardcoded localhost port in Railway.

## C. Create the EF Core migration (required before deployment)

The original ZIP did not contain a Migrations folder. The app must have a migration before it can create the Reservations table with `Database.Migrate()` or `Update-Database`.

In Visual Studio Package Manager Console, with this project selected as the Default project, run:

```powershell
Add-Migration InitialCreate
```

Check that a `Migrations` folder containing the migration and model snapshot was created. Then run:

```powershell
Update-Database
```

This applies the migration to your local MySQL database using the local connection string.

Commit and push the newly generated `Migrations` folder to GitHub. The application code in this package intentionally does not call `EnsureCreated()`: `EnsureCreated()` bypasses EF migration history and makes later migrations difficult.

For Railway, deploy the migration files and apply the migration to the Railway MySQL database. One simple approach for a school project is to temporarily use the Railway connection string as the active `DefaultConnection` when running `Update-Database`, then restore the local connection string. Alternatively, run `dotnet ef database update` with the Railway connection string supplied securely as an environment variable. Never commit the Railway password.

The application does not automatically run schema migrations at startup. This is deliberate: applying schema changes automatically on every web instance startup can cause deployment/race problems. Ensure the Railway database migration has been applied before testing CRUD.

## D. GitHub and Railway deployment

1. Extract this package.
2. Copy/merge its contents into your existing repository, preserving the original `.git` directory in your local clone. Do not replace or delete `.git`.
3. Generate the EF migration as described above.
4. Commit and push the changed files and the new `Migrations` folder.
5. In Railway, connect the GitHub repository to the ASP.NET application service.
6. Confirm Railway detects the root `Dockerfile` (beside the `.csproj`).
7. Confirm the MySQL variables are linked.
8. Deploy and inspect Build Logs and Runtime Logs.
9. Generate a public domain in Railway Networking.

## E. Test

- `https://YOUR-RAILWAY-DOMAIN/health` — confirms the web process responds.
- `https://YOUR-RAILWAY-DOMAIN/health/database` — checks whether the app can reach MySQL.
- `https://YOUR-RAILWAY-DOMAIN/` — opens the reservation system.

The database health endpoint does not reveal credentials or connection details.

## Existing application behavior preserved

- Reservation list, details, create, edit, and delete.
- Pending / Approved / Rejected statuses.
- Approve and reject actions.
- Sample operating hours 8:00 AM–5:00 PM.
- Maximum attendee validation (100).
- Prevention of overlapping pending or approved bookings.
- Existing Bootstrap views and service/interface structure.

## Important security warning

The original project README and layout indicate that authentication and role-based authorization have not yet been implemented. The approve, reject, and delete actions are therefore not protected as administrator-only actions. Do not use this public deployment with real personal data or treat it as a production-ready booking service until authentication, authorization, and appropriate access controls are added.

## Framework note

This repository targets .NET 9 to preserve compatibility with the existing project. .NET 9 reached end of support on November 10, 2026. Plan an upgrade to a supported framework when your Visual Studio/tooling environment supports it.

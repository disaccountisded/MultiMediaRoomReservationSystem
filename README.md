# Multi Media Room Reservation System

A beginner-friendly ASP.NET Core MVC project for a single school venue: the Multi Media Room.

## Technology
- Visual Studio 2022
- ASP.NET Core MVC / .NET 9
- Entity Framework Core 9
- SQL Server LocalDB (default connection)
- Bootstrap 5.3.3 (CDN)
- C# OOP: inheritance, encapsulation through model properties/validation, abstraction through an interface, and polymorphism through the service implementation

## Before opening
Install:
1. Visual Studio 2022 with the **ASP.NET and web development** workload.
2. .NET 9 SDK.
3. SQL Server Express LocalDB (or change the connection string to your SQL Server instance).

> .NET 9 reached end of support on November 10, 2026. This project targets .NET 9 because that was the requested framework. For a new production project, consider a supported LTS framework and matching EF Core version.

## Open in Visual Studio
1. Extract the ZIP file.
2. Open `MultiMediaRoomReservationSystem.csproj` in Visual Studio 2022.
3. Wait for NuGet package restore to finish.
4. Check that the `DefaultConnection` in `appsettings.json` matches your SQL Server/LocalDB installation.

## Check LocalDB
Open Visual Studio's Terminal and run:

```powershell
sqllocaldb info
```

If `MSSQLLocalDB` is listed, the default connection string should work. If LocalDB is unavailable, install SQL Server Express LocalDB or replace the server name in `appsettings.json`.

## Create the database
In Visual Studio, open **Tools > NuGet Package Manager > Package Manager Console**.

Run:

```powershell
Add-Migration InitialCreate
Update-Database
```

This creates `MultiMediaRoomReservationDB` and its `Reservations` table.

## Run
Press `Ctrl + F5`. The application opens on the Reservations page.

## Current functionality
- Create, list, view, edit, and delete reservation requests.
- Approve or reject pending requests.
- Validate required fields and email format.
- Enforce sample operating hours (8:00 AM–5:00 PM).
- Prevent overlapping non-rejected reservations.
- Store reservation data in SQL Server through EF Core.

## Important: development-only security
Authentication and role-based authorization have intentionally not been added yet, as requested. At this stage, the admin actions (approve, reject, delete) are not protected by login. Do not deploy this application publicly or enter real personal information until authentication, authorization, and ownership checks have been implemented.

## Assumptions to confirm with your school
- Operating hours are currently set to 8:00 AM–5:00 PM.
- Maximum attendees are currently set to 100.
- Reservation requests require administrator approval.
- The venue is fixed to the Multi Media Room.

Change these values to match the school's actual policies.

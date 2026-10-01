# BinLord

An ASP.NET Core web app that reminds you when your bins need collecting.

Enter one or more bin schedules (bin name, colour, a known collection date, and
how often it repeats), and the homepage shows the next collection date for
each bin, highlighting any that are due today or tomorrow.

## Features

- Add, edit, and delete bin schedules
- Each schedule has a name, colour tag, an anchor collection date, a repeat
  interval (in weeks), and optional notes
- The homepage automatically works out the next collection date for every
  bin and lists them soonest-first
- Collections due today or tomorrow are highlighted with a banner and badge

## Tech stack

- ASP.NET Core 8 MVC
- Entity Framework Core with SQLite
- Bootstrap for styling

## Getting started

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
cd src/BinLord
dotnet run
```

The database (SQLite) is created and migrated automatically on first run.
Once started, open the URL printed in the console (e.g.
`http://localhost:5236`).

## Project structure

```
BinLord.sln
src/BinLord/
  Controllers/   MVC controllers (Home, BinSchedules)
  Models/        BinSchedule entity and view models
  Data/          EF Core DbContext
  Migrations/    EF Core migrations
  Views/         Razor views
```

## Running behind a reverse proxy

BinLord trusts `X-Forwarded-Proto`/`X-Forwarded-Host`/`X-Forwarded-For`
headers from any upstream, so the scheme and host it sees (used in the
RSS/calendar feed links) reflect your reverse proxy's public address rather
than Kestrel's internal binding — this assumes BinLord is only reachable
through that proxy, not exposed directly as well. If your proxy doesn't set
those headers, or you want feed links to always use a specific domain, set
a **Base URL override** on the Settings page instead.

## Database migrations

Migrations are managed with the EF Core CLI tool, installed locally via the
repo's tool manifest:

```bash
dotnet tool restore
dotnet dotnet-ef migrations add <Name> --project src/BinLord --startup-project src/BinLord
```

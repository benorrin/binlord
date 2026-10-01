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
`http://localhost:5236`) — the first visit walks you through creating an
admin account.

## Accounts and permissions

- **Admin**: everything, including Settings and managing users.
- **Editor**: can view and modify bin schedules and history, but not Settings or users.
- **Viewer**: can only view.

There's no self-service registration — admins create accounts on the Users
page. On the Settings page, **Schedules are publicly visible** controls
whether anyone can view the homepage/schedules/history without logging in
(viewing only; changes always require an Editor or Admin account). When
turned off, the RSS and calendar feed links gain an access token so feed
readers and calendar apps — which can't log in — keep working. The
`/health` and `/api/status` monitoring endpoints are always reachable
without logging in, regardless of this setting.

## Running with Docker

```bash
docker compose up -d --build
```

This builds the image, starts BinLord on `http://localhost:8080`, and
persists the SQLite database in a named volume (`binlord-data`, mounted at
`/data`) so it survives rebuilds and `docker compose down`. Migrations run
automatically on startup, same as running it directly.

To use a different host port, set `BINLORD_PORT` (e.g. `BINLORD_PORT=9000
docker compose up -d`), or set `TZ` (e.g. `TZ=Europe/London`) for container
log timestamps — the app's own notion of "today" comes from the Time zone
setting on the Settings page, not this.

All other configuration (app name, time zone, notifications, access control,
etc.) is done through the Settings page once it's running, not through
environment variables.

## Project structure

```
BinLord.sln
Dockerfile, docker-compose.yml
src/BinLord/
  Controllers/   MVC controllers (Home, BinSchedules)
  Models/        BinSchedule entity and view models
  Data/          EF Core DbContext
  Migrations/    EF Core migrations
  Views/         Razor views
```

## Notifications and monitoring

On the Settings page you can configure:

- **Email and/or [ntfy](https://ntfy.sh) notifications**, sent automatically
  when a bin needs putting out, and again when it needs bringing in (checked
  every 15 minutes in the background; each reminder is only sent once per
  occurrence).
- An **Uptime Kuma** (or any other monitor) integration: `/health` always
  returns 200 OK, and `/api/status` returns JSON with `"ok":true` when
  nothing needs doing or `"ok":false` when a bin needs attention. Point an
  HTTP(s) monitor with a Keyword check for `"ok":true` at it to route alerts
  through whatever notification channels you've already set up in Kuma,
  without configuring email/ntfy in BinLord at all.

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

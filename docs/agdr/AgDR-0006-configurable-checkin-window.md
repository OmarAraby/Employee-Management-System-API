# Check-in window moved to configuration (kept as a business rule)

> In the context of fixing the employee journey (ticket #27) where the hard-coded 07:30–09:00 check-in window read as a bug during off-hours testing, I decided to keep the window as a real business rule but source it from configuration (`Attendance:CheckInStart`/`CheckInEnd`) and expose it via an endpoint, rather than remove it or leave it hard-coded, to achieve tunability without a code deploy and a UI that can show the rule, accepting that the values still live in env/compose config rather than a per-tenant admin screen.

## Context

- `CheckInAsync` hard-coded `new TimeSpan(7,30,0)` / `new TimeSpan(9,0,0)`. Testing outside that 90-minute Egypt-time window always returned `TimeRestriction`, which looked like a defect.
- Operator decision (during planning): keep the rule, surface it in the UI, and make it configurable.

## Options Considered

| Option | Pros | Cons |
|--------|------|------|
| Remove the window | No off-hours friction | Drops a real business rule (attendance punctuality) |
| Keep hard-coded | Simplest | Can't change without a deploy; invisible to the UI |
| **Config-sourced + exposed (chosen)** | Tunable via env/compose; UI can display it via `GET /attendance/check-in-window`; safe defaults preserved | Values are app-level config, not a per-tenant admin UI (acceptable for now) |

## Decision

Chosen: **config-sourced**. `AttendanceManager.GetCheckInWindow()` reads `Attendance:CheckInStart`/`CheckInEnd` (parsed as `TimeSpan`, defaulting to 07:30/09:00 when unset or unparseable). `CheckInAsync` uses it; `GET /api/attendance/check-in-window` returns it so the frontend shows the allowed hours without hard-coding them again. Wired through compose (`Attendance__CheckInStart/End`) + `.env.example`.

## Consequences

- Change the window by editing `.env` / compose env — no rebuild.
- The `TimeRestriction` message now interpolates the configured window.
- Future: a per-tenant/admin-managed schedule would move this out of static config — recorded so it isn't assumed done.

## Artifacts

- Ticket: #27 · Part of the employee E2E fix plan (PR A).

# Attendance check-out rules — anytime after check-in, working hours computed on the server

> In the context of building the attendance check-out endpoint (ticket #17), facing the choice of when check-out is allowed and how working hours are recorded, I decided to allow check-out at any time after the day's check-in and compute `WorkingHours` server-side as (checkout − checkin), to achieve a simple, honest record of the actual worked interval, accepting that overnight shifts and missed check-outs are not modelled in v1.

## Context

- Check-**in** is constrained to a 07:30–09:00 window (existing rule in `CheckInAsync`).
- The `Attendance` row already has `CheckOutTime` and `WorkingHours` (both nullable); nothing writes them yet.
- Check-out must be tied to the caller's own record (ownership), consistent with the #9 IDOR fixes.

## Options Considered

| Question | Options | Choice |
|----------|---------|--------|
| When may an employee check out? | (a) fixed window like check-in; (b) any time after today's check-in | **(b) anytime after check-in** — a work day has no fixed end; forcing a window would reject legitimate late finishes |
| How are working hours recorded? | (a) client sends duration; (b) server computes checkout − checkin | **(b) server-computed** — the client must never be trusted for a recorded metric (same principle as check-in spoofing, #9) |
| Second check-out? | (a) overwrite; (b) reject | **(b) reject** ("already checked out today") — a check-out is a one-time event; overwriting would let someone inflate/deflate hours |
| No check-in today? | (a) create a bare row; (b) reject | **(b) reject** ("must check in first") — check-out without check-in is meaningless |

## Decision

Chosen as marked above. Check-out is `POST /api/attendance/check-out` `[Authorize(Roles="Employee")]`, keyed to the caller's identity (`callerId` from the token). The manager loads today's attendance for the caller, rejects if missing or already checked out, else sets `CheckOutTime` = local (Egypt) time-of-day and `WorkingHours` = `(checkOut − checkIn).TotalHours`, then persists via a new `IAttendanceRepository.UpdateAsync`.

## Consequences

- New `IAttendanceRepository.UpdateAsync(Attendance)` (mirrors `EmployeeRepository.UpdateAsync`) — the repo had no update path.
- **Not modelled in v1** (recorded so it isn't assumed): overnight shifts (checkout next calendar day), auto-checkout for a forgotten check-out, and editing a check-out. If these become requirements, revisit the "reject second check-out" and same-day assumptions.
- Working hours is a plain elapsed interval — breaks/lunch are not deducted.

## Artifacts

- Ticket: #17 · Initiative: `projects/initiatives/ems-completion.md` (private portfolio repo)

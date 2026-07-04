# Attendance report — inline CSV generation over a library

> In the context of adding a monthly attendance report the admin can hand to payroll (ticket #5), facing the choice of how to produce the CSV, I decided to build the CSV inline with a small RFC-4180-escaping `StringBuilder` helper rather than add a CSV library, to achieve zero new dependencies for a handful of columns, accepting that a richer future export (many columns, streaming, Excel) would justify revisiting.

## Context

- The report is the validated "smallest version that proves the value" slice: a monthly attendance CSV for one employee, downloadable by an Admin.
- The data already exists: `AttendanceManager.GetMonthlyAttendanceAsync` returns `APIResult<IEnumerable<AttendanceListDto>>`, and after #9 an Admin may call it for any employee.
- The project has **no** CSV/Excel dependency today, and `IFileService` is upload-only — a download/generate path is net-new either way.

## Options Considered

| Option | Pros | Cons |
|--------|------|------|
| **Inline `StringBuilder` + RFC-4180 escaping helper** | Zero dependencies; ~30 lines; trivially unit-testable; fits the 6-column shape | Hand-rolled escaping (mitigated by a single tested helper); not suited to huge/streamed exports |
| CsvHelper library | Battle-tested escaping; maps POCOs automatically | New dependency + version-drift surface for 6 columns; overkill |
| EPPlus / ClosedXML (xlsx) | Rich Excel output | Heavy dependency; licensing (EPPlus non-commercial); the ticket asked for CSV |

## Decision

Chosen: **inline `StringBuilder` with a single escaping helper**, because the export is small and fixed-shape, and a dependency-free implementation is the simplest thing that fully satisfies the requirement. Escaping lives in one place (`CsvExport.Field`) so it's tested once and reused.

## Consequences

- A new `EmployeeManagementSys.BL/Utils/CsvExport.cs` helper (field escaping + row join) — reusable if other CSV exports appear.
- A new manager method returns the CSV bytes; the controller returns `File(bytes, "text/csv", filename)`. Authorization + data reuse the existing monthly path (Admin→any, Employee→own).
- If exports later need many columns, multiple sheets, or streaming for large datasets, revisit and adopt CsvHelper — recorded so it isn't re-litigated.

## Artifacts

- Ticket: #5 · Initiative: `projects/initiatives/ems-completion.md` (private portfolio repo), Milestone 4

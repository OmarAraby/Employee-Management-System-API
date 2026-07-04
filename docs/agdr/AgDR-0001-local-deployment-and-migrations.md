# Local deployment shape — full compose stack with containerized SQL Server, migrations applied at startup

> In the context of making the EMS pair deployable with one command (initiative ems-completion, #4), facing the choice of where the database lives and how the schema reaches an empty container database, I decided to run SQL Server inside the compose stack (with a documented escape hatch to the developer's local SQL Server) and to apply EF migrations at application startup, to achieve a zero-prerequisite `docker compose up` experience, accepting single-instance-only migration semantics and a slower first boot.

## Context

- The initiative's success criterion is literally "deploy locally via one `docker compose up`" — any host-machine prerequisite (a running local SQL Server, a manually-created database, a manual `dotnet ef database update`) breaks that.
- The operator DOES have a local SQL Server installed — so the containerized default must not lock that workflow out.
- Containers boot against an empty database; the startup seed (roles + admin) requires the schema to already exist.

## Options Considered

### A. Where the database lives

| Option | Pros | Cons |
|--------|------|------|
| **Containerized SQL Server 2022 (chosen default)** | Zero prerequisites; identical stack on any machine; healthcheck-gated startup; data isolated in a named volume | ~1.5 GB image; first pull is slow; duplicates a DB the developer may already run locally |
| Developer's local SQL Server | Reuses existing install; familiar tooling (SSMS) | Every machine needs SQL Server pre-installed + configured; breaks the one-command criterion; connection config varies per machine |
| Hybrid (supported escape hatch) | Best of both — the compose default stays self-contained, but a developer can point the API at their local server | Slightly more docs; `--no-deps` invocation is non-obvious |

**Escape hatch (documented in `.env.example`):** set `EMS_DB_CONNECTION` to `Server=host.docker.internal;…` and start only `api` + `web` via `docker compose up --no-deps api web`. TCP/IP must be enabled on the local SQL instance.

### B. How the schema reaches the database

| Option | Pros | Cons |
|--------|------|------|
| **`Database.MigrateAsync()` at startup (chosen)** | Zero extra moving parts; idempotent; works for both fresh containers and existing local DBs; seed can rely on schema existing | Single-instance only — concurrent replicas racing `MigrateAsync()` can deadlock; a failed migration takes the app down at boot (which is arguably correct: fail loud) |
| Init container / separate migration step | Clean separation; safe for multi-replica | More compose complexity for a local dev stack with exactly one API instance |
| Manual `dotnet ef database update` | Explicit control | A human prerequisite — breaks one-command |

## Decision

Chosen: **containerized SQL Server (with the local-SQL escape hatch) + migrate-on-startup**, because the deployment target is a single-instance local development stack where zero-prerequisite startup outweighs multi-replica concerns.

## Consequences

- If this stack ever runs with multiple API replicas (cloud deployment), migrate-on-startup MUST move to an init step first — recorded here so it isn't rediscovered in an incident.
- First `docker compose up` is slow (image pulls + migration + seed); subsequent boots are fast.
- The local-SQL workflow stays supported but is opt-in via `.env`, keeping the default path machine-independent.

## Artifacts

- Ticket: #4 · PR: #6 · Sibling: OmarAraby/Employee-Management-System-ng#6 / PR ng#7
- Initiative: `projects/initiatives/ems-completion.md` (private portfolio repo), Milestone 1

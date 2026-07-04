# Docker build restores the API project, not the solution

> In the context of adding an `EmployeeManagementSys.Tests` project to the solution (ticket #3), facing a broken image build (the Dockerfile ran `dotnet restore` on the `.sln`, which now pulls in a test project whose csproj isn't copied into the build context), I decided to restore/publish the API project directly rather than the solution, to achieve a runtime image that contains only production code, accepting that a new production project added later must be added to the Dockerfile's COPY list explicitly.

## Context

- The API `Dockerfile` did `dotnet restore EmployeeManagementSys.API.sln`, relying on the solution to enumerate projects.
- Adding the test project to the solution (so `dotnet test` and CI discover it) meant the solution restore now required the test csproj — which the Dockerfile deliberately doesn't copy — and the build failed.

## Options Considered

| Option | Pros | Cons |
|--------|------|------|
| **Restore the API csproj directly** | Runtime image never pulls test projects or their deps (Moq, coverlet); smaller, faster, correct for production | A future *production* project must be added to the Dockerfile COPY list by hand (project references are transitive, so this only matters for genuinely new top-level projects) |
| Copy the test csproj into the build context too | Solution restore keeps working | Ships test-only dependencies' restore into the prod image build; wrong layering — tests don't belong in the runtime image |
| Keep test project out of the solution | Dockerfile unchanged | `dotnet test` / CI can't discover the tests via the solution; defeats the point of #3 |

## Decision

Chosen: **restore the API project directly** (`dotnet restore EmployeeManagementSys.API/EmployeeManagementSys.API.csproj`), because a production image should build from production code only, and .NET restores project references transitively (API→BL→DL) — so nothing production is lost. `.dockerignore` also excludes `EmployeeManagementSys.Tests/` and `TestResults/`.

## Consequences

- The test project lives in the solution (discovered by `dotnet test` + CI) but never enters the runtime image.
- A newly added **top-level production** project (not reachable via the API's project references) must be added to the Dockerfile's COPY + restore lines — an acceptable, explicit step.

## Artifacts

- Ticket: #3 · Refines the deployment shape in AgDR-0001.

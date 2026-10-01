# ProTodo

A small REST API Task Manager (projects containing tasks), built for the Backend Developer take-home assignment.

## Technology
- C# / ASP.NET Core Web API (.NET 10, controllers)
- Entity Framework Core 10 with SQL Server (migrations included)
- xUnit + `WebApplicationFactory` integration tests against SQL Server LocalDB
- Claude Code as the AI coding assistant (see [AI-USAGE-NOTES.md](AI-USAGE-NOTES.md))

## Features
- Create/list projects
- Create/list/filter project tasks (by status)
- Update/delete tasks
- API-key authentication on every endpoint
- Input validation with a consistent `{ "message": "..." }` error shape
- 54 automated integration tests

## Prerequisites
- **.NET SDK 10.0** (developed and verified with 10.0.401)
- **SQL Server**: the default development and test configuration uses **SQL Server LocalDB** (instance `MSSQLLocalDB`, installed with Visual Studio or the SQL Server Express LocalDB installer). LocalDB is Windows-only; on another platform point `ConnectionStrings__DefaultConnection` at any SQL Server (the test suite's connection string is in `tests/ProTodo.Api.Tests/ApiFactory.cs`).

## Configuration
| Setting | Purpose | Where it comes from |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection | `appsettings.Development.json` (LocalDB, integrated security, no password) or env var `ConnectionStrings__DefaultConnection` |
| `ApiKey` | Expected `X-API-Key` value | .NET user-secrets or env var `ApiKey` |

There is **no default API key**. The app refuses to start if `ApiKey` is empty or missing, and the key is never logged or committed.

Set the key for local development with user-secrets (stored outside the repository):
```bash
dotnet user-secrets set ApiKey "<choose-a-long-random-value>" --project src/ProTodo.Api
```

## Database Setup
Run once (and again after pulling new migrations). This creates the `ProTodo` database on LocalDB:
```bash
dotnet tool restore
dotnet ef database update --project src/ProTodo.Api
```
`dotnet tool restore` installs the pinned `dotnet-ef` tool from `dotnet-tools.json`.

## Run
```bash
dotnet restore
dotnet build
dotnet run --project src/ProTodo.Api
```
The `http` launch profile listens on `http://localhost:5062` in the Development environment.

Example:
```bash
curl -X POST http://localhost:5062/api/projects \
  -H "X-API-Key: <your key>" -H "Content-Type: application/json" \
  -d '{"name":"Website Development"}'
```

## Authentication
Every request, including unknown routes, must send:
```http
X-API-Key: <API_KEY>
```
A missing, empty, repeated or wrong key returns `401 Unauthorized`. The key is compared in constant time (SHA-256 digests compared with `CryptographicOperations.FixedTimeEquals`).

## API Endpoints
| Method | Endpoint | Description | Success |
|---|---|---|---|
| POST | `/api/projects` | Create project (`{ "name": "..." }`) | 201 |
| GET | `/api/projects` | List projects | 200 |
| POST | `/api/projects/{id}/tasks` | Create task (`{ "title": "...", "description": "..." }`); status starts as `Pending` | 201 |
| GET | `/api/projects/{id}/tasks` | List tasks; optional `?status=Pending\|InProgress\|Completed` (case-insensitive) | 200 |
| PUT | `/api/tasks/{id}` | Update task (`title`, `description`, `status`) | 200 |
| DELETE | `/api/tasks/{id}` | Delete task | 204 |

Rules:
- Project `name`: required, not blank, max 100 characters (trimmed before saving).
- Task `title`: required, not blank, max 200 characters. `description`: optional, max 2000 characters (blank is stored as null).
- Task `status`: one of `Pending`, `InProgress`, `Completed`; numbers and other strings are rejected.
- `PUT` replaces `title`, `description` and `status` (omitting `description` clears it). A task cannot be moved to another project; any `projectId` in the body is ignored. `updatedAtUtc` is refreshed.
- An invalid or empty `?status=` filter returns 400 instead of an empty list.
- Lists are ordered by `id`; there is no pagination.

## Error Handling
All errors use `{ "message": "..." }`.

| Status | When | Example message |
|---|---|---|
| 400 | Validation failure, malformed JSON, invalid status | `Project name is required.` / `Status must be one of: Pending, InProgress, Completed.` |
| 401 | Missing/invalid `X-API-Key` | `A valid X-API-Key header is required.` |
| 404 | Project or task does not exist | `Project was not found.` / `Task was not found.` |
| 500 | Unexpected failure | `An unexpected error occurred.` (details are logged server-side only) |

## Testing
```bash
dotnet test
```
54 tests run the real application through `WebApplicationFactory` against SQL Server LocalDB. Each test class gets its own uniquely named database (`ProTodo_Test_<guid>`), created from the EF Core migrations and dropped when the class finishes, so tests never touch the development database and do not depend on order. Test settings (API key, connection string) are injected in code; no secrets are needed.

Scenarios covered:
- **Authentication**: no key, empty key, wrong key, near-miss keys → 401 on all six endpoints and unknown routes; valid key → success.
- **Projects**: create (201, trimming), missing/blank/too-long name (400), malformed JSON (400 with no parser details), list.
- **Tasks**: create (201, default `Pending`), nonexistent project (404), invalid title/description (400), list scoped to a project, status filtering (Pending/InProgress/Completed, case-insensitive), invalid/empty status filter (400), update (200, `updatedAtUtc`, cannot change project, clears description), update of a nonexistent task (404), invalid status on update (400, data unchanged), delete (204), repeated/nonexistent delete (404).
- **Error handling**: an unreachable database produces a generic 500 with no internals in the body.

## Project Structure
```text
ProTodo.sln
src/ProTodo.Api/
  Controllers/    thin controllers (routes, status codes)
  Services/       ProjectService, TaskService (business/database logic)
  Data/           AppDbContext, EF Core configuration, Migrations/
  Entities/       Project, TaskItem, TaskItemStatus, field limits
  DTOs/           request/response contracts
  Configuration/  API key middleware, validation error response
tests/ProTodo.Api.Tests/   integration tests
docs/                      assignment specification and workflow
```

## Design Decisions
- **No repository layer / MediatR / CQRS**: services use `AppDbContext` directly; EF Core is already the abstraction.
- **`TaskItem` and `TaskItemStatus`** instead of `Task`/`TaskStatus`, which would clash with `System.Threading.Tasks` types under implicit usings.
- **Status stored as a string** in the database (readable, safe against enum reordering); validated by name only.
- **Validation** uses data annotations plus a custom `InvalidModelStateResponseFactory` so every 400 has the same shape. JSON binding errors are replaced with a generic message so parser internals are not leaked.
- **API key middleware** runs before routing, so unknown routes also return 401 and reveal nothing.
- **Fail-fast configuration**: a missing `ApiKey` stops startup instead of leaving the API open.
- **Migrations are applied explicitly** (`dotnet ef database update`), not automatically at startup.
- **Cascade delete** on `Task → Project` is configured in the model although project deletion is out of scope.
- Services return `null`/`false` for "not found" and controllers map that to 404.

## AI-Assisted Development
Claude Code was the primary AI coding assistant. See [AI-USAGE-NOTES.md](AI-USAGE-NOTES.md).

## GitHub Repository
Repository name: `ProTodo` — https://github.com/KashanAli-AIDev/ProTodo

## Known Limitations
- One shared API key; no users, roles, or per-project authorization.
- No pagination, project update/delete, or rate limiting.
- LocalDB is Windows-only, so the default test setup targets Windows; other platforms need a SQL Server connection string change in `ApiFactory`.
- The API listens on plain HTTP in the default launch profile; terminate TLS in front of it for real deployments.

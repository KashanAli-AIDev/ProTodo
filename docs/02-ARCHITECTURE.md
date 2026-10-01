# ProTodo — Architecture

## Structure
```text
ProTodo/
├── src/
│   └── ProTodo.Api/
│       ├── Controllers/
│       ├── Services/
│       ├── Data/
│       ├── Entities/
│       ├── DTOs/
│       ├── Configuration/
│       └── Program.cs
├── tests/
│   └── ProTodo.Api.Tests/
├── docs/
├── CLAUDE.md
├── README.md
└── AI-USAGE-NOTES.md
```

## Flow
```text
HTTP Request
    ↓
Controller
    ↓
Service
    ↓
EF Core DbContext
    ↓
SQL Server
```

## Controllers
Define routes, receive requests, call services, and return responses. Do not contain substantial business/database logic.

## Services
Handle application behavior: project creation/listing, task creation/listing/filtering/update/delete, and resource existence checks.

## DTOs
Use API request/response DTOs rather than exposing persistence entities directly.

Recommended:
- CreateProjectRequest
- ProjectResponse
- CreateTaskRequest
- UpdateTaskRequest
- TaskResponse

## Data
Contains EF Core DbContext, configuration, relationships, and migrations.

## Dependency Injection
Use built-in ASP.NET Core DI.

## Error Handling
At minimum:
- 400 invalid input
- 401 missing/invalid API key
- 404 missing resource
- 500 unexpected server failure

Do not expose internal exception details.

## Database
Use EF Core asynchronously. Appropriate methods include `FirstOrDefaultAsync`, `ToListAsync`, `AddAsync`, and `SaveChangesAsync`. Use `AsNoTracking()` where appropriate for reads.

Do not add a generic repository merely to wrap EF Core.

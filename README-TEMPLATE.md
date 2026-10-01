# ProTodo

A small REST API Task Manager built for the Backend Developer take-home assignment.

## Technology
- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- xUnit
- Claude Code

## Features
- Create/list projects
- Create/list/filter project tasks
- Update/delete tasks
- API-key authentication
- Validation
- Automated tests

## Prerequisites
Document the exact .NET SDK and SQL Server requirements.

## Configuration
Document exact final configuration. Example placeholder:
```text
ConnectionStrings__DefaultConnection=<connection-string>
ApiKey=<secret>
```
Never put a real secret in this file.

## Database Setup
Document the exact working migration/database command.

## Run
Document exact commands, for example:
```bash
dotnet restore
dotnet build
dotnet run --project src/ProTodo.Api
```

## Authentication
All required endpoints require:
```http
X-API-Key: <API_KEY>
```
Explain safe development configuration.

## API Endpoints
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/projects` | Create project |
| GET | `/api/projects` | List projects |
| POST | `/api/projects/{id}/tasks` | Create task |
| GET | `/api/projects/{id}/tasks` | List/filter tasks |
| PUT | `/api/tasks/{id}` | Update task |
| DELETE | `/api/tasks/{id}` | Delete task |

## Testing
```bash
dotnet test
```
Describe important scenarios.

## Error Handling
Document 400, 401, 404, and 500 behavior.

## Project Structure
Briefly explain the final structure.

## Design Decisions
Document only meaningful decisions.

## AI-Assisted Development
Claude Code was the primary AI coding assistant. See `AI-USAGE-NOTES.md`.

## GitHub Repository
Repository name: `ProTodo`

Add the final public/shared repository link before submission.

## Known Limitations
Document only genuine remaining limitations.

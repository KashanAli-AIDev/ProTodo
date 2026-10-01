# ProTodo — Requirements

## Objective
Build a small REST API for a Task Manager: a to-do list organized into projects.

## Required Endpoints
| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/projects` | Create a project |
| GET | `/projects` | List all projects |
| POST | `/projects/{id}/tasks` | Add a task to a project |
| GET | `/projects/{id}/tasks` | List project tasks with status filtering |
| PUT | `/tasks/{id}` | Update a task |
| DELETE | `/tasks/{id}` | Delete a task |

## Required Capabilities
- Relational database
- Input validation
- Meaningful 400/404/etc. responses
- Basic authentication or API-key check
- Automated tests
- README with setup/run instructions
- Incremental Git history
- AI-assisted development evidence

## Selected Implementation
- SQL Server
- Entity Framework Core
- API key authentication

## Out of Scope
Do not add frontend, registration, multi-user identity, roles, JWT/refresh tokens, project deletion, notifications, real-time features, pagination, microservices, CQRS/MediatR, cloud deployment, or other unnecessary features.

## Acceptance Checklist
- [ ] All six endpoints work
- [ ] Status filtering works
- [ ] Relational database works
- [ ] Validation works
- [ ] Missing resources return 404
- [ ] Authentication/API key is enforced
- [ ] Meaningful automated tests exist
- [ ] README is complete
- [ ] AI Usage Notes are complete
- [ ] Git history shows incremental work

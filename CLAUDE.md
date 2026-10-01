# ProTodo — Claude Code Master Instructions

## Mission
Build the Backend Developer take-home assignment as a small, clean REST API for a Task Manager with Projects and Tasks. Claude Code is the primary development partner, but all generated code must be reviewed, tested, and corrected by the developer.

## Required Reading Order
Before implementation, read:
1. `CLAUDE.md`
2. `docs/00-PROJECT-CONTEXT.md`
3. `docs/01-REQUIREMENTS.md`
4. `docs/02-ARCHITECTURE.md`
5. `docs/03-API-CONTRACT.md`
6. `docs/04-DATA-MODEL.md`
7. `docs/05-SECURITY-VALIDATION.md`
8. `docs/06-TESTING.md`
9. `docs/07-DEVELOPMENT-WORKFLOW.md`

Also inspect `README-TEMPLATE.md` and `AI-USAGE-NOTES.md`.

If documents conflict, identify the conflict before implementing.

## Canonical Naming
- Product/project: `ProTodo`
- Git repository: `ProTodo`
- Solution: `ProTodo.sln`
- API project: `ProTodo.Api`
- Test project: `ProTodo.Api.Tests`

Do not invent another project name.

## Repository Creation
Repository creation is an explicit operational action controlled by the developer's first prompt.

Do not independently create, rename, delete, or publish repositories unless explicitly authorized.

When authorized, use the connected Git account to create/use the `ProTodo` repository, preserve incremental commits, and never expose credentials or tokens.

## Recommended Stack
- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- xUnit
- ASP.NET Core integration testing

## Engineering Rules
- Keep the solution small and appropriate for a 3–5 hour assignment.
- Prefer simple, readable code.
- Keep controllers thin.
- Put application/business behavior in services.
- Use DTOs for API contracts.
- Use async database operations.
- Validate external input.
- Return appropriate HTTP status codes.
- Never expose stack traces or internal exception details.
- Never commit secrets.
- Use configuration/environment variables for secrets.
- Use safe parameterized database access.
- Keep relationships and constraints explicit.
- Avoid abstractions without a real benefit.

## Do Not Over-Engineer
Do not add CQRS, MediatR, event sourcing, microservices, generic repositories, complex DDD layers, registration, JWT identity, roles, refresh tokens, frontend, notifications, real-time features, Docker, cloud infrastructure, pagination, or caching unless genuinely required.

## AI Review Rule
Never accept generated code merely because it compiles. Review correctness, security, validation, HTTP semantics, database behavior, error handling, tests, maintainability, and unnecessary complexity.

If Claude makes a real mistake/suboptimal decision, identify it, correct it, test it, and record the real example in `AI-USAGE-NOTES.md`. Never fabricate an AI mistake.

## Development Order
1. Repository/project setup
2. Solution scaffolding
3. Database/domain
4. Project endpoints
5. Task endpoints/filtering
6. Authentication
7. Validation/error handling
8. Automated tests
9. README
10. AI Usage Notes
11. Final review

After each meaningful increment: inspect diff, build, test, correct, then commit.

## Final Gate
Run:
```bash
dotnet clean
dotnet build
dotnet test
```
Then verify all six endpoints, authentication, validation, database setup, tests, documentation, no secrets, and useful Git history.

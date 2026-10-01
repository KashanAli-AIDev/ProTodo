# ProTodo — Testing Strategy

## Goal
Prove that the API works and that important failure paths are handled.

## Recommended Tools
- xUnit
- ASP.NET Core integration testing
- isolated test database

## Minimum Coverage
### Authentication
- no key → 401
- wrong key → 401
- correct key succeeds

### Projects
- valid creation → 201
- invalid/missing name → 400
- listing → 200

### Tasks
- create under existing project → 201
- create under nonexistent project → 404
- list → 200
- filter by Pending
- filter by Completed
- invalid status handled consistently
- update existing → 200
- update nonexistent → 404
- delete existing → 204
- delete nonexistent → 404

## Test Isolation
Tests must not depend on order and must not use a real developer/production database.

## Quality
Every test should prove meaningful behavior with specific assertions and important failure coverage.

## Final Command
The README must document the working full-suite command:
```bash
dotnet test
```

Final gate:
```bash
dotnet build
dotnet test
```
Then manually exercise all six endpoints.

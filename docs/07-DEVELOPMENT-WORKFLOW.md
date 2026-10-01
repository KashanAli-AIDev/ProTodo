# ProTodo — Claude Code Development Workflow

## Phase 0 — Understand
Read `CLAUDE.md` and all specification documents. Inspect the repository. Identify ambiguity before implementation.

## Phase 1 — Repository and Solution
After explicit developer authorization:
- create/use GitHub repository `ProTodo` through the connected Git account;
- create the solution and API/test projects;
- build and inspect.

Commit:
```text
Initialize ProTodo solution
```

## Phase 2 — Database and Domain
Implement Project, Task, relationship, DbContext, EF Core configuration, and migration/database setup.

Commit:
```text
Add project and task data model
```

## Phase 3 — Projects
Implement POST and GET `/api/projects`.

Commit:
```text
Implement project endpoints
```

## Phase 4 — Tasks
Implement task create/list/filter/update/delete.

Commit:
```text
Implement task endpoints
```

## Phase 5 — Authentication/Error Handling
Implement API-key protection and safe error handling.

Commit:
```text
Add API key authentication and error handling
```

## Phase 6 — Tests
Add meaningful integration tests.

Commit:
```text
Add API integration tests
```

## Phase 7 — Documentation
Complete README and AI Usage Notes.

Commit:
```text
Add project documentation and AI usage notes
```

## Phase 8 — Final Review
Run:
```bash
dotnet clean
dotnet build
dotnet test
```
Inspect:
```bash
git status
git diff
git log --oneline
```
Search for secrets and verify all requirements.

## Prompting
Prefer narrow prompts:
```text
Implement only POST /api/projects according to the API contract.
Use the existing architecture and DTO conventions. Do not add unrelated
features. After implementation, explain the files changed and list edge
cases I should review.
```

Avoid:
```text
Build the entire application perfectly.
```

## Review Loop
```text
Prompt → Generate → Inspect diff → Build → Test → Review → Correct → Commit
```

## AI Evidence
Keep actual prompts. Record at least 2–3 in `AI-USAGE-NOTES.md`, plus one real AI mistake/suboptimal decision and how it was corrected. Never fabricate evidence.

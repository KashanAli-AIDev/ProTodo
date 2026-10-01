# AI Usage Notes — ProTodo

> Written from the actual Claude Code session. Prompts are quoted from the session (the second is shortened where marked); issues listed are ones that really occurred.

## AI Tool
**Tool:** Claude Code (Claude Sonnet 5.5, desktop app, Code tab)

## How this was run
Two developer prompts drove the whole project. The first was a read-only planning step; the second authorized Claude to carry out all phases autonomously while the developer was not supervising. Consequently, review during implementation was performed by Claude itself (build, tests, manual `curl` checks, diff inspection, mutation checks of the tests). The developer's contribution was the specification documents, the decisions on ambiguities and the instruction to proceed. The developer should still read the code before relying on it.

## Prompt 1
### Actual prompt
```text
You are working on my Backend Developer take-home assignment.

First, read CLAUDE.md and then read every Markdown file under docs/.
Also inspect README-TEMPLATE.md and AI-USAGE-NOTES.md.

Do not start implementing the application yet.

The canonical project/repository name is ProTodo.

I explicitly authorize you to create the GitHub repository named ProTodo
using the Git account/connector that is already logged in and available
to you.

After creating or confirming the repository, initialize the project
workspace and Git repository as appropriate.

Then give me:
1. Your understanding of the assignment.
2. The final implementation plan.
3. The proposed solution/project structure.
4. The development phases you will follow.
5. Any ambiguity or concern you found in the specification.

Do not implement the application yet. Wait for my approval before
starting Phase 1 development.
```
### What Claude produced
Read all documents, created a private GitHub repo `ProTodo` with `gh`, initialized git with a docs-only commit and a .NET `.gitignore`, and wrote a plan. It flagged ten ambiguities, among them `/projects` vs `/api/projects`, the test database choice, invalid status filters, error shape, PUT semantics and field limits.
### My review
The developer answered the open questions (LocalDB for tests, 400 for invalid status filter, uniform `{ "message" }` error shape, keep the repo private until the end).

## Prompt 2 (shortened)
### Actual prompt
```text
Proceed autonomously with the entire ProTodo take-home assignment. I am authorizing you to continue through all implementation phases without asking me for routine approval.
[...]
Test database: Use SQL Server LocalDB using the available MSSQLLocalDB instance. [...] Do NOT switch to EF Core InMemory. [...]
Invalid status filter: [...] return 400 Bad Request with a clear message identifying the valid status values. Do not silently return an empty list for an invalid status.
Validation error shape: Use the consistent error shape { "message": "..." } [...]
Repository visibility: Keep the GitHub repository private during development. [...] At the very end, after all implementation, tests, documentation, security checks, and final verification have passed, make the repository public [...]
[... eight phases, each with: implement, build, test, inspect diff, review, fix, commit ...]
Do not claim anything that you did not actually verify.
Proceed now.
```
(The full prompt also listed the phase-by-phase requirements, the autonomy rules and the final report format.)
### What Claude produced
Eight phases with one commit each: solution, data model, project endpoints, task endpoints, API-key authentication and error handling, 54 integration tests, README, and a final audit.
### My review (what was actually checked)
After each phase Claude built the solution and exercised the running API with `curl`. The test suite was run repeatedly, and two deliberate code mutations were applied to confirm the tests fail when behaviour breaks (status filter ignored; API key check disabled; later also UTC conversion removed). Test databases were confirmed to be dropped afterwards.

## Prompt 3 (shortened)
### Actual prompt
```text
Proceed with the final completion and submission of the ProTodo project autonomously. [...]
1. Delete README-TEMPLATE.md [...]
2. Complete verification [...] test all six API endpoints [...] Keep the repository private while performing all verification. [...]
7. Make the repository public [...] only after [everything passes].
```
### What Claude produced
Set the repository back to private (it had already been made public at the end of the previous round), deleted `README-TEMPLATE.md`, re-ran restore/clean/build/test, ran a scripted end-to-end check of all six endpoints against the real LocalDB development database (auth, 400/404 cases, status filtering, `updatedAtUtc`, persistence across an application restart, delete), repeated the secret scan, then pushed and made the repository public again.
### Result
No application defects were found in this round. The only problem was in Claude's own check script (below).

## Real AI Corrections
These are the problems Claude itself introduced and then found and fixed during the session.

### 1. Invalid JSON in `appsettings.Development.json`
- **What Claude originally did:** wrote the LocalDB connection string with a single backslash (`(localdb)\MSSQLLocalDB`) inside a JSON file. A first automated fix with `sed` also produced a single backslash.
- **Why it was wrong:** a lone `\M` is an invalid JSON escape, so the configuration failed to load.
- **How it was detected:** `dotnet ef migrations add` failed with "Failed to load configuration from file"; `dotnet run` printed the JSON parse error.
- **What changed:** the file now has `\\`, written with a small Python script (shell quoting was part of the cause).
- **Verification:** migrations created, `dotnet ef database update` succeeded, the schema was inspected with `sqlcmd` (column types, lengths, foreign key).

### 2. Leaky, noisy 400 messages for malformed JSON
- **What Claude originally did:** the first custom validation response joined all ModelState messages.
- **Why it was suboptimal:** for malformed bodies the message exposed parser internals (`Path: $ | LineNumber: 0 | BytePositionInLine: 1`, .NET type names) and always appended a meaningless "The request field is required.".
- **How it was detected:** manual `curl` with `{bad`, a wrong-typed `name`, and an empty body.
- **What changed:** `ValidationErrorResponse` returns one fixed generic message for any JSON binding error, and drops the parameter-level "request" noise.
- **Verification:** repeated the `curl` calls; the integration tests `Create_project_with_malformed_body_returns_400_without_parser_details` assert the exact message.

### 3. UTC timestamps lost their `Z` when read back
- **What Claude originally did:** stored `DateTime` values with no handling of `DateTimeKind`.
- **Why it was wrong:** the create response ended in `Z`, but the list response (loaded from SQL Server, `Kind=Unspecified`) did not, so clients would interpret the same field inconsistently.
- **How it was detected:** comparing the `POST` and `GET /api/projects` responses in a manual run.
- **What changed:** a value converter in `AppDbContext.ConfigureConventions` marks every `DateTime` as UTC when read.
- **Verification:** `dotnet ef migrations has-pending-model-changes` reports no schema change; a test asserts `Kind == Utc` on listed projects, and removing the converter makes that test fail.

### 4. Empty `?status=` silently ignored
- **What Claude originally did:** treated a null `status` parameter as "no filter".
- **Why it was wrong:** MVC binds `?status=` (empty) to null, so the request returned every task, which contradicts the requirement not to silently ignore invalid status values.
- **How it was detected:** the manual check of filter edge cases after implementing the endpoints returned `200` with all tasks for `?status=`.
- **What changed:** the controller checks `Request.Query.ContainsKey("status")` and validates the raw value.
- **Verification:** `curl` now returns 400; `List_with_invalid_status_returns_400_listing_valid_values` covers `""` and `%20`.

### Smaller process slips (no product impact)
- A multi-file `bash` heredoc batch was rejected by the shell parser and wrote nothing; the files were recreated with the file-writing tool.
- A "missing API key stops startup" check initially looked like it failed, because `dotnet run` applied the launch profile (Development, with user-secrets). It was rerun with `--no-launch-profile` and the fail-fast behaviour was confirmed.

- In the final end-to-end script, Claude saved responses to `/tmp/o` (Git Bash path) and read them with Windows Python, which cannot see that path. Most checks reported false failures (404 for a project ID that was never parsed). Claude noticed the Python `FileNotFoundError`, concluded the fault was the script (not the API), moved the response file to a path both tools can read, and re-ran: all 28 checks passed.

### Decision rather than mistake
The spec's names `Task` and `TaskStatus` collide with `System.Threading.Tasks` under implicit usings, so the entity and enum are `TaskItem` and `TaskItemStatus`. This was a deliberate choice, noted in the README.

## Test-driven findings
The 54 integration tests passed the first time they were run, so they exposed no application defects. Their value was confirmed by mutation checks instead: disabling the status filter, disabling the API-key check and removing the UTC converter each made tests fail.

## Review Criteria
Generated code was reviewed for:
- correctness
- HTTP status codes
- validation
- authentication/security
- secret handling
- database integrity
- exception handling
- test quality
- maintainability
- unnecessary complexity

Compilation alone was not treated as proof of correctness.

## Development Approach
Claude Code worked phase by phase (setup, data model, projects, tasks, authentication/error handling, tests, documentation, audit). Each phase was built, exercised, reviewed, corrected and then committed.

## Final Reflection
AI accelerated the boilerplate (entities, DTOs, services, test scaffolding) and the repetitive edge-case coverage. Its mistakes were the sort a quick manual run catches: escaping, error-message hygiene, date kinds and empty query values. Those were found by running the API rather than by reading code, so the developer should rerun the manual checks and review the diff personally before submitting.

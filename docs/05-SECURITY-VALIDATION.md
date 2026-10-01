# ProTodo — Security and Validation

## API Key
Use:
```http
X-API-Key: <secret>
```

The expected key must come from configuration/environment variables/user secrets.

Never commit, log, or return the real key.

Missing/invalid key → `401 Unauthorized`.

## Input Validation
Validate:
- project name
- task title
- task status
- optional description limits

Reject null, empty, whitespace-only, unsupported status, malformed data, and unreasonable input sizes.

## Resource Checks
- Creating a task requires an existing project.
- Updating/deleting requires an existing task.

## Database Safety
Use EF Core safely. Do not concatenate user input into SQL. Raw SQL is unnecessary unless a genuine reason appears.

## Exception Safety
Do not expose stack traces, SQL, connection strings, or implementation details to clients.

## Scope
No users, roles, ownership, or complex authorization are required.

## Review Checklist
- [ ] API key enforced everywhere
- [ ] Missing/wrong key → 401
- [ ] No secrets committed
- [ ] Input validated
- [ ] Database operations safe
- [ ] Internal exceptions not exposed

# ProTodo — API Contract

## Base Route
Use `/api`.

Final routes:
```text
POST   /api/projects
GET    /api/projects
POST   /api/projects/{id}/tasks
GET    /api/projects/{id}/tasks
PUT    /api/tasks/{id}
DELETE /api/tasks/{id}
```

## Authentication
Every endpoint requires:
```http
X-API-Key: <API_KEY>
```
Missing/invalid key: `401 Unauthorized`.

## POST /api/projects
Request:
```json
{ "name": "Website Development" }
```
Success: `201 Created`.

Name is required, non-whitespace, and bounded to a reasonable maximum length.

## GET /api/projects
Success: `200 OK`.

Example:
```json
[
  { "id": 1, "name": "Website Development" }
]
```

## POST /api/projects/{id}/tasks
Request:
```json
{
  "title": "Implement login API",
  "description": "Create the authentication endpoint"
}
```
Success: `201 Created`.
Missing project: `404 Not Found`.

## GET /api/projects/{id}/tasks
Success: `200 OK`.
Missing project: `404 Not Found`.

Optional filter:
```text
GET /api/projects/1/tasks?status=Completed
```

## PUT /api/tasks/{id}
Request:
```json
{
  "title": "Implement login API",
  "description": "Authentication endpoint completed",
  "status": "Completed"
}
```
Success: `200 OK`.
Missing task: `404 Not Found`.

## DELETE /api/tasks/{id}
Success: `204 No Content`.
Missing task: `404 Not Found`.

## Status
Recommended values:
- `Pending`
- `InProgress`
- `Completed`

Unsupported status values must not silently become valid data.

## Error Shape
Use a simple consistent response such as:
```json
{ "message": "Project was not found." }
```
Standard ASP.NET Core validation responses are acceptable if clear and documented.

Never return stack traces, database details, secrets, or connection strings.

## HTTP Semantics
- 201 create
- 200 read/update
- 204 delete
- 400 invalid input
- 401 authentication failure
- 404 missing resource
- 500 unexpected failure

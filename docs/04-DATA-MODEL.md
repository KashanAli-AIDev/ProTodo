# ProTodo — Data Model

## Relationship
```text
Project
   1
   │
   N
 Task
```

A Task belongs to one Project.

## Project
| Field | Type | Rules |
|---|---|---|
| Id | int | Primary key |
| Name | string | Required, bounded |
| CreatedAtUtc | DateTime | Recommended |

## Task
| Field | Type | Rules |
|---|---|---|
| Id | int | Primary key |
| ProjectId | int | Required foreign key |
| Title | string | Required, bounded |
| Description | string | Optional, bounded |
| Status | enum/string | Required |
| CreatedAtUtc | DateTime | Recommended |
| UpdatedAtUtc | DateTime | Recommended |

## Status
Recommended enum:
```text
Pending
InProgress
Completed
```

## Referential Integrity
`Task.ProjectId → Project.Id`.

A Task cannot be created for a nonexistent Project.

Project deletion is not required; do not spend assignment time on complex deletion behavior.

## EF Core
Configure required properties, maximum lengths, relationship, and only justified indexes.

Use migrations and document the exact setup command in README.

## Data Integrity Tests
Cover nonexistent project, nonexistent task, invalid status, missing project name, and missing task title.

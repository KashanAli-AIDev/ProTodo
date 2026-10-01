namespace ProTodo.Api.DTOs;

public record TaskResponse(
    int Id,
    int ProjectId,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

using System.ComponentModel.DataAnnotations;
using ProTodo.Api.Entities;

namespace ProTodo.Api.DTOs;

public class UpdateTaskRequest
{
    [Required(ErrorMessage = "Task title is required.")]
    [StringLength(FieldLimits.TaskTitle, ErrorMessage = "Task title must be at most {1} characters.")]
    public string? Title { get; set; }

    [StringLength(FieldLimits.TaskDescription, ErrorMessage = "Task description must be at most {1} characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Task status is required.")]
    [ValidTaskStatus]
    public string? Status { get; set; }
}

public sealed class ValidTaskStatusAttribute : ValidationAttribute
{
    // Null/empty is reported by [Required]; this only rejects non-empty unsupported values.
    public override bool IsValid(object? value) =>
        value is not string s || string.IsNullOrWhiteSpace(s) || TaskItemStatusParser.TryParse(s, out _);

    public override string FormatErrorMessage(string name) => TaskItemStatusParser.ValidValuesMessage;
}

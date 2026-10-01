using System.ComponentModel.DataAnnotations;
using ProTodo.Api.Entities;

namespace ProTodo.Api.DTOs;

public class CreateTaskRequest
{
    [Required(ErrorMessage = "Task title is required.")]
    [StringLength(FieldLimits.TaskTitle, ErrorMessage = "Task title must be at most {1} characters.")]
    public string? Title { get; set; }

    [StringLength(FieldLimits.TaskDescription, ErrorMessage = "Task description must be at most {1} characters.")]
    public string? Description { get; set; }
}

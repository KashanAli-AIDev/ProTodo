using System.ComponentModel.DataAnnotations;
using ProTodo.Api.Entities;

namespace ProTodo.Api.DTOs;

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(FieldLimits.ProjectName, ErrorMessage = "Project name must be at most {1} characters.")]
    public string? Name { get; set; }
}

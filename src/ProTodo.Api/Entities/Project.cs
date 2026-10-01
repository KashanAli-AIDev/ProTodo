namespace ProTodo.Api.Entities;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public List<TaskItem> Tasks { get; set; } = [];
}

namespace ProTodo.Api.Entities;

// Named TaskItemStatus (not TaskStatus) to avoid clashing with System.Threading.Tasks.TaskStatus.
public enum TaskItemStatus
{
    Pending,
    InProgress,
    Completed
}

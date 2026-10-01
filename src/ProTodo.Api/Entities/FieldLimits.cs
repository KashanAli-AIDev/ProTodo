namespace ProTodo.Api.Entities;

// Single source of truth for string lengths, shared by validation and the database schema.
public static class FieldLimits
{
    public const int ProjectName = 100;
    public const int TaskTitle = 200;
    public const int TaskDescription = 2000;
}

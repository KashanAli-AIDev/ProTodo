namespace ProTodo.Api.Entities;

public static class TaskItemStatusParser
{
    public static readonly string ValidValuesMessage =
        $"Status must be one of: {string.Join(", ", Enum.GetNames<TaskItemStatus>())}.";

    // Accepts only the enum names (case-insensitive). Enum.TryParse is avoided because it also
    // accepts numbers such as "1" or "99", which would let unsupported values through.
    public static bool TryParse(string? value, out TaskItemStatus status)
    {
        foreach (var name in Enum.GetNames<TaskItemStatus>())
        {
            if (string.Equals(name, value?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                status = Enum.Parse<TaskItemStatus>(name);
                return true;
            }
        }

        status = default;
        return false;
    }
}

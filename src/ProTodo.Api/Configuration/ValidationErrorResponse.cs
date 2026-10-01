using Microsoft.AspNetCore.Mvc;
using ProTodo.Api.DTOs;

namespace ProTodo.Api.Configuration;

public static class ValidationErrorResponse
{
    // Keys starting with "$" come from the JSON binder, whose messages expose parser internals.
    // Replaces the default ProblemDetails body so 400s use the same { "message": "..." } shape as other errors.
    public static IActionResult Create(ActionContext context)
    {
        // The "request" parameter-required error is noise next to a JSON error, so a JSON error wins.
        if (context.ModelState.Keys.Any(k => k.StartsWith('$')))
            return new BadRequestObjectResult(
                new ErrorResponse("The request body is not valid JSON or has fields of the wrong type."));

        var messages = new List<string>();

        foreach (var (key, entry) in context.ModelState)
        {
            foreach (var error in entry.Errors)
            {
                if (key == "request" || string.IsNullOrEmpty(key))
                    messages.Add("A valid JSON request body is required.");
                else
                    messages.Add(string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The request is invalid." : error.ErrorMessage);
            }
        }

        return new BadRequestObjectResult(new ErrorResponse(string.Join(" ", messages.Distinct())));
    }
}

using Microsoft.AspNetCore.Mvc;
using ProductApi.Application.Common;

namespace ProductApi.Extensions;

/// <summary>
/// Extension methods to map Result pattern to ASP.NET Core ActionResult.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an ActionResult with appropriate HTTP status code.
    /// </summary>
    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return result.Error.ToProblemDetails();
    }

    /// <summary>
    /// Converts a Result of T to an ActionResult of T with appropriate HTTP status code.
    /// </summary>
    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return result.Error.ToProblemDetails();
    }

    /// <summary>
    /// Maps Result to ActionResult with custom success status code.
    /// </summary>
    public static ActionResult ToActionResult<T>(this Result<T> result, Func<T, ActionResult> onSuccess)
    {
        return result.IsSuccess 
            ? onSuccess(result.Value) 
            : result.Error.ToProblemDetails();
    }

    /// <summary>
    /// Converts an Error to a ProblemDetails ActionResult.
    /// </summary>
    public static ActionResult ToProblemDetails(this Error error)
    {
        var (statusCode, title) = error.Type switch
        {
            ErrorType.Validation => (400, "Validation Error"),
            ErrorType.NotFound => (404, "Resource Not Found"),
            ErrorType.Conflict => (409, "Conflict"),
            ErrorType.Unauthorized => (401, "Unauthorized"),
            ErrorType.Forbidden => (403, "Forbidden"),
            _ => (400, "Bad Request")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = error.Message,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        if (error.ValidationErrors != null && error.ValidationErrors.Any())
        {
            problemDetails.Extensions["errors"] = error.ValidationErrors;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Pattern matching extension for cleaner controller code.
    /// </summary>
    public static ActionResult Match<T>(
        this Result<T> result,
        Func<T, ActionResult> onSuccess,
        Func<Error, ActionResult> onFailure)
    {
        return result.IsSuccess 
            ? onSuccess(result.Value) 
            : onFailure(result.Error);
    }

    /// <summary>
    /// Simplified pattern matching with default error handling.
    /// </summary>
    public static ActionResult Match<T>(
        this Result<T> result,
        Func<T, ActionResult> onSuccess)
    {
        return result.Match(onSuccess, error => error.ToProblemDetails());
    }

    /// <summary>
    /// Pattern matching extension for Result (non-generic) for cleaner controller code.
    /// </summary>
    public static ActionResult Match(
        this Result result,
        Func<ActionResult> onSuccess,
        Func<Error, ActionResult> onFailure)
    {
        return result.IsSuccess 
            ? onSuccess() 
            : onFailure(result.Error);
    }

    /// <summary>
    /// Simplified pattern matching for Result (non-generic) with default error handling.
    /// </summary>
    public static ActionResult Match(
        this Result result,
        Func<ActionResult> onSuccess)
    {
        return result.Match(onSuccess, error => error.ToProblemDetails());
    }
}

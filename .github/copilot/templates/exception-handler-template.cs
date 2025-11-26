using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace ProductApi.Middleware;

/// <summary>
/// Template for global exception handling middleware
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        
        _logger.LogError(
            exception,
            "An unhandled exception occurred. TraceId: {TraceId}",
            traceId);

        var (statusCode, title, detail) = MapException(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = _environment.IsDevelopment() ? detail : "An error occurred processing your request.",
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId
            }
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private (int statusCode, string title, string detail) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentException 
                => (400, "Bad Request", exception.Message),
            
            UnauthorizedAccessException 
                => (401, "Unauthorized", "Authentication required"),
            
            KeyNotFoundException 
                => (404, "Not Found", exception.Message),
            
            InvalidOperationException 
                => (409, "Conflict", exception.Message),
            
            NotSupportedException 
                => (422, "Unprocessable Entity", exception.Message),
            
            _ => (500, "Internal Server Error", exception.Message)
        };
    }
}

// Extension method to register the handler
public static class GlobalExceptionHandlerExtensions
{
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}

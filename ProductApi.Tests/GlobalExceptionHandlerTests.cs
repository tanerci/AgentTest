using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ProductApi.Common;
using System.IO;
using System.Text.Json;
using Xunit;

namespace ProductApi.Tests;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_InDevelopment_IncludesExceptionDetails()
    {
        // Arrange
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Development");
        
        var handler = new GlobalExceptionHandler(logger, environment);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        
        var exception = new InvalidOperationException("Test error message");

        // Act
        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        Assert.StartsWith("application/json", httpContext.Response.ContentType ?? "");
        
        // Read response body
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("An error occurred while processing your request", problemDetails.Title);
        Assert.Equal("Test error message", problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_ExcludesExceptionDetails()
    {
        // Arrange
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Production");
        
        var handler = new GlobalExceptionHandler(logger, environment);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        
        var exception = new InvalidOperationException("Sensitive error details");

        // Act
        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        
        // Read response body
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(problemDetails);
        Assert.Null(problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_LogsException()
    {
        // Arrange
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Production");
        
        var handler = new GlobalExceptionHandler(logger, environment);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        
        var exception = new InvalidOperationException("Test error");

        // Act
        await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert - verify that Log method was called
        logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
}

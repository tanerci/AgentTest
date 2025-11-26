using Microsoft.Extensions.Logging;

namespace ProductApi.Services;

/// <summary>
/// Template for implementing logging in service classes
/// </summary>
public class ServiceTemplate
{
    private readonly ILogger<ServiceTemplate> _logger;

    public ServiceTemplate(ILogger<ServiceTemplate> logger)
    {
        _logger = logger;
    }

    public async Task<Result> PerformOperationAsync(string operationId)
    {
        // Log method entry with context
        _logger.LogInformation("Starting operation {OperationId}", operationId);

        try
        {
            // Log debug information if needed
            _logger.LogDebug("Processing operation {OperationId} with parameters", operationId);

            // Perform operation
            await Task.Delay(100); // Simulated work

            // Log successful completion
            _logger.LogInformation("Operation {OperationId} completed successfully", operationId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            // Log error with exception and context
            _logger.LogError(ex, "Error occurred during operation {OperationId}", operationId);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<TData>> PerformOperationWithDataAsync<TData>(string operationId)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationId"] = operationId,
            ["Timestamp"] = DateTime.UtcNow
        }))
        {
            _logger.LogInformation("Starting data operation");

            try
            {
                // Simulated work
                var data = default(TData);
                
                _logger.LogInformation("Data operation completed");
                return Result<TData>.Success(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Data operation failed");
                return Result<TData>.Failure(ex.Message);
            }
        }
    }

    // Performance logging example
    public async Task<Result> PerformLongRunningOperationAsync()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting long-running operation");
            
            // Simulated work
            await Task.Delay(1000);
            
            stopwatch.Stop();
            _logger.LogInformation(
                "Long-running operation completed in {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Long-running operation failed after {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
            
            return Result.Failure(ex.Message);
        }
    }
}

// Simple Result pattern for error handling
public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
}

public class Result<T> : Result
{
    public T Data { get; }

    private Result(bool isSuccess, T data, string error) : base(isSuccess, error)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new Result<T>(true, data, string.Empty);
    public new static Result<T> Failure(string error) => new Result<T>(false, default, error);
}

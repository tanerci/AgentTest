namespace ProductApi.Application.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Use this instead of throwing exceptions for expected failures.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot have an error");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

/// <summary>
/// Represents the result of an operation that returns a value on success.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("Cannot access Value on a failed result");
            return _value!;
        }
    }

    private Result(bool isSuccess, T? value, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static new Result<T> Failure(Error error) => new(false, default, error);

    // Convenience methods for common error types
    public static Result<T> NotFound(string message = "Resource not found") 
        => Failure(Error.NotFound(message));

    public static Result<T> ValidationError(string message, Dictionary<string, string[]>? errors = null) 
        => Failure(Error.Validation(message, errors));

    public static Result<T> Conflict(string message) 
        => Failure(Error.Conflict(message));

    public static Result<T> Unauthorized(string message = "Unauthorized access") 
        => Failure(Error.Unauthorized(message));

    public static Result<T> Forbidden(string message = "Forbidden") 
        => Failure(Error.Forbidden(message));

    // Pattern matching support
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }
}

/// <summary>
/// Represents an error with type, message, and optional validation details.
/// </summary>
public sealed class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }

    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    private Error(string code, string message, ErrorType type, Dictionary<string, string[]>? validationErrors = null)
    {
        Code = code;
        Message = message;
        Type = type;
        ValidationErrors = validationErrors;
    }

    public static Error NotFound(string message, string code = "NotFound") 
        => new(code, message, ErrorType.NotFound);

    public static Error Validation(string message, Dictionary<string, string[]>? errors = null, string code = "Validation") 
        => new(code, message, ErrorType.Validation, errors);

    public static Error Conflict(string message, string code = "Conflict") 
        => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string message, string code = "Unauthorized") 
        => new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string message, string code = "Forbidden") 
        => new(code, message, ErrorType.Forbidden);

    public static Error Failure(string message, string code = "Failure") 
        => new(code, message, ErrorType.Failure);

    public static Error Custom(string code, string message, ErrorType type) 
        => new(code, message, type);
}

/// <summary>
/// Types of errors for semantic handling and HTTP mapping.
/// </summary>
public enum ErrorType
{
    None = 0,
    Failure = 1,
    Validation = 2,
    NotFound = 3,
    Conflict = 4,
    Unauthorized = 5,
    Forbidden = 6
}

namespace ProductApi.Domain.Exceptions;

/// <summary>
/// Base exception for domain-related errors.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when a domain validation rule is violated.
/// </summary>
public class DomainValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    public DomainValidationException(string message) : base(message)
    {
    }

    public DomainValidationException(string message, Dictionary<string, string[]> validationErrors) 
        : base(message)
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }
}

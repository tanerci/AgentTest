using ProductApi.Domain.Entities;

namespace ProductApi.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate root.
/// Defines the contract for user persistence operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user in the repository.
    /// </summary>
    Task UpdateAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the given username exists.
    /// </summary>
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
}

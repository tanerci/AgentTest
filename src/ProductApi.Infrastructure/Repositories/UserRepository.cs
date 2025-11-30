using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Repositories;
using ProductApi.Infrastructure.Persistence.Models;

namespace ProductApi.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IUserRepository.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly Persistence.AppDbContext _context;

    public UserRepository(Persistence.AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var model = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return model == null ? null : MapToDomainEntity(model);
    }

    public async Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var model = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        return model == null ? null : MapToDomainEntity(model);
    }

    public async Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        var model = MapToDataModel(user);
        
        _context.Users.Add(model);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDomainEntity(model);
    }

    public async Task UpdateAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        var model = await _context.Users.FindAsync([user.Id], cancellationToken)
            ?? throw new InvalidOperationException($"User with ID {user.Id} not found");

        model.Username = user.Username;
        model.PasswordHash = user.PasswordHash;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
    }

    private static UserEntity MapToDomainEntity(User model)
    {
        // Use the Hydrate factory method to safely reconstruct the domain entity from persistence
        return UserEntity.Hydrate(
            model.Id,
            model.Username,
            model.PasswordHash);
    }

    private static User MapToDataModel(UserEntity entity)
    {
        return new User
        {
            Id = entity.Id,
            Username = entity.Username,
            PasswordHash = entity.PasswordHash
        };
    }
}

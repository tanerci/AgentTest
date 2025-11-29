using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Repositories;

namespace ProductApi.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IUserRepository.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
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
        var model = await _context.Users.FindAsync(new object[] { user.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"User with ID {user.Id} not found");

        model.Username = user.Username;
        model.PasswordHash = user.PasswordHash;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
    }

    private static UserEntity MapToDomainEntity(Models.User model)
    {
        // Using reflection to set private properties (for domain entity hydration from persistence)
        var entity = (UserEntity)Activator.CreateInstance(typeof(UserEntity), nonPublic: true)!;
        
        var idProp = typeof(UserEntity).GetProperty(nameof(UserEntity.Id));
        var usernameProp = typeof(UserEntity).GetProperty(nameof(UserEntity.Username));
        var passwordHashProp = typeof(UserEntity).GetProperty(nameof(UserEntity.PasswordHash));

        idProp!.SetValue(entity, model.Id);
        usernameProp!.SetValue(entity, model.Username);
        passwordHashProp!.SetValue(entity, model.PasswordHash);

        return entity;
    }

    private static Models.User MapToDataModel(UserEntity entity)
    {
        return new Models.User
        {
            Id = entity.Id,
            Username = entity.Username,
            PasswordHash = entity.PasswordHash
        };
    }
}

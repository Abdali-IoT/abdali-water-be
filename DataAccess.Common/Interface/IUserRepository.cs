using DataAccess.Common.Entity;

namespace DataAccess.Common.Interface;

public interface IUserRepository
{
    public Task<IEnumerable<UserEntity>> GetAllUsersAsync(CancellationToken cancellationToken);

    public Task<UserEntity?> ResolveGoogleUserAsync(string subject, string firstSignInEmail,
        CancellationToken cancellationToken);

    public Task<IEnumerable<UserEntity>> FindByIdAsync(Guid userId, CancellationToken cancellationToken);
}
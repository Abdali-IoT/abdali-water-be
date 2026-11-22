using Common.Dto;

namespace Service.Interface;

public interface IUserService
{
    public Task<IReadOnlyList<UserDto?>> GetAllUsersAsync(CancellationToken cancellationToken);

    public Task<UserDto?> ResolveGoogleUserAsync(string subject, string firstSignInEmail,
        CancellationToken cancellationToken);

    public Task<UserDto?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);
}
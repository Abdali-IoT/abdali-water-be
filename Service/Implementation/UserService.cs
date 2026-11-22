using Common.Dto;
using Common.Mapper;
using DataAccess.Common.Interface;
using Microsoft.Extensions.Logging;
using Service.Interface;

namespace Service.Implementation;

public sealed class UserService(IUserRepository userRepository, ILogger<UserService> logger) : IUserService
{
    public async Task<IReadOnlyList<UserDto?>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        var list = await userRepository.GetAllUsersAsync(cancellationToken);
        return list.ToDto();
    }

    public async Task<UserDto?> ResolveGoogleUserAsync(string subject, string firstSignInEmail, CancellationToken cancellationToken)
    {
        var entity = await userRepository.ResolveGoogleUserAsync(subject, firstSignInEmail, cancellationToken);
        return entity.ToDto();
    }

    public async Task<UserDto?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var entity = await userRepository.FindByIdAsync(userId, cancellationToken);
        return entity.FirstOrDefault().ToDto();
    }
}
using System.Data;
using Dapper;
using DataAccess.Common.Entity;
using DataAccess.Common.Interface;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DataAcess.Repository;

public sealed class UserRepository(Func<IDbConnection> connectionFactory, ILogger<UserRepository> logger)
    : IUserRepository
{
    public async Task<IEnumerable<UserEntity>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = """
                           SELECT id, name, email, created_at, google_subject, google_sign_in_enabled
                           FROM users
                           """;

        using var connection = connectionFactory();
        var list = await connection.QueryAsync<UserEntity>(sql);
        return list;
    }

    public async Task<UserEntity?> ResolveGoogleUserAsync(string subject, string firstSignInEmail,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindByGoogleSubjectAsync(subject, cancellationToken);
        if (user is not null || string.IsNullOrEmpty(firstSignInEmail))
        {
            return user;
        }

        const string sql = """
                           UPDATE users
                           SET google_subject = @subject
                           WHERE btrim(email) <> ''
                             AND lower(btrim(email)) = lower(btrim(@firstSignInEmail))
                             AND google_subject IS NULL
                             AND google_sign_in_enabled
                           RETURNING id, name, email, created_at, google_subject, google_sign_in_enabled
                           """;

        using var connection = connectionFactory();
        UserEntity? updatedUser = null;

        try
        {
            updatedUser = await connection.ExecuteScalarAsync<UserEntity>(sql, new { subject, firstSignInEmail });
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation
                                                  && exception.ConstraintName == "uq_users_google_subject")
        {
            // A concurrent request linked this Google subject first.
            logger.LogWarning("Request has already linked this google subject: {@subject}", subject);
        }

        return updatedUser;
    }

    private async Task<UserEntity?> FindByGoogleSubjectAsync(string subject,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = """
                           SELECT id, name, email, created_at, google_subject, google_sign_in_enabled
                           FROM users
                           WHERE google_subject = @subject
                           """;

        using var connection = connectionFactory();
        var user = await connection.QueryFirstOrDefaultAsync<UserEntity>(sql, new { subject });
        return user;
    }

    public async Task<IEnumerable<UserEntity>> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = """
                           SELECT u.id, u.name, u.email, u.created_at, u.google_subject, u.google_sign_in_enabled, 
                                  d.id as DeviceId, d.id, d.hardware_id, d.name, d.firmware_version, d.last_seen_at, d.created_at
                           FROM users u
                           left join user_devices ud on u.id = ud.user_id
                           left join devices d on d.id = ud.device_id 
                           WHERE u.id = @userId
                           """;

        using var connection = connectionFactory();
        var userDict = new Dictionary<Guid, UserEntity>();
        var list = await connection.QueryAsync<UserEntity, DeviceEntity?, UserEntity>(
            sql,
            (user, device) =>
            {
                // 1. Check if the parent Order is already cached
                if (!userDict.TryGetValue(user.Id, out var currentUser))
                {
                    currentUser = user;
                    userDict.Add(currentUser.Id, currentUser);
                }

                // 2. Add the child item (guard against null if using a LEFT JOIN)
                if (device is not null)
                {
                    currentUser.Devices.Add(device);
                }

                return currentUser;
            },
            param: new { userId },
            splitOn: "DeviceId"
        );

        return list;
    }
}
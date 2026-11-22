using Common.Dto;
using DataAccess.Common.Entity;

namespace Common.Mapper;

public static class EntityToDtoMapper
{
    public static IReadOnlyList<DeviceDto> ToDto(this IReadOnlyList<DeviceEntity>? entities)
    {
        return entities is null ? new List<DeviceDto>() : [.. entities.Select(e => e.ToDto())];
    }

    public static IReadOnlyList<UserDto?> ToDto(this IEnumerable<UserEntity> entities) =>
        [.. entities.Select(entity => entity.ToDto())];


    public static DeviceDto ToDto(this DeviceEntity entity) =>
        new DeviceDto(entity.Id, entity.HardwareId, entity.Name,
            entity.FirmwareVersion!, entity.LastSeenAt, entity.CreatedAt);

    public static UserDto? ToDto(this UserEntity? entity) => entity is null
        ? null
        : new UserDto(entity.Id, entity.Name, entity.Email, entity.CreatedAt,
            entity.GoogleSubject, entity.GoogleSignInEnabled, entity.Devices.ToDto()!);
}
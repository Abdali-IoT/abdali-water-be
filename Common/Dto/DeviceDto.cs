using DataAccess.Common.Enum;

namespace Common.Dto;

public record DeviceDto(
    Guid Id, 
    Guid HardwareId, 
    string Name, 
    string DeviceType,
    string FirmwareVersion, 
    DateTime LastSeenAt, 
    DateTime CreatedAt
    );
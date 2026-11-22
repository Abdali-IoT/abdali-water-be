namespace Common.Dto;

public record DeviceDto(
    Guid Id, 
    Guid HardwareId, 
    string Name, 
    string FirmwareVersion, 
    DateTime LastSeenAt, 
    DateTime CreatedAt
    );
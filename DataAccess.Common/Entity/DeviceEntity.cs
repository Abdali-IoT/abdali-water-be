using DataAccess.Common.Enum;

namespace DataAccess.Common.Entity;

public class DeviceEntity
{
    public Guid Id { get; set; }
    public Guid HardwareId { get; set; }
    public required string Name { get; set; }
    public required DeviceTypeEnum  DeviceType { get; set; }
    public string? FirmwareVersion { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
using DataAccess.Common.Enum;

namespace DataAccess.Common.Entity;

public class LatestDeviceReportEntity
{
    public Guid DeviceId { get; set; }
    public required string MeasurementCode { get; set; }
    public required string MeasurementName { get; set; }
    public DeviceTypeEnum DeviceType { get; set; }
    public required string DeviceName { get; set; }
    public string? Unit { get; set; }
    public required string ValueKind { get; set; }
    public decimal? ValueNumber { get; set; }
    public bool? ValueBoolean { get; set; }
    public string? ValueText { get; set; }
    public DateTime MeasuredAt { get; set; }
    public DateTime ReceivedAt { get; set; }
}
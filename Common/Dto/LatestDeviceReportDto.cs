using DataAccess.Common.Enum;

namespace Common.Dto;

public record LatestDeviceReportDto(
    Guid DeviceId,
    string MeasurementCode,
    string MeasurementName,
    string DeviceType,
    string DeviceName,
    string? Unit,
    string ValueType,
    decimal? ValueNumber,
    string? ValueText,
    bool? ValueBoolean,
    DateTime MeasuredAt,
    DateTime ReceivedAt
    );
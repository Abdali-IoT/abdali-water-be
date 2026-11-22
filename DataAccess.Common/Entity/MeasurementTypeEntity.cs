
using DataAccess.Common.Enum;

namespace DataAccess.Common.Entity;

public class MeasurementTypeEntity
{
    public long Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Unit { get; set; }
    public MeasurementTypeValueKindEnum ValueKind { get; set; }
}
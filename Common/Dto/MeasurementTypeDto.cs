using DataAccess.Common.Enum;

namespace Common.Dto;

public record MeasurementTypeDto(
    long Id, 
    string Code, 
    string Name,
    string Unit,
    MeasurementTypeValueKindEnum ValueKind
    );
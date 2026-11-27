using Common.Dto;

namespace Service.Interface;

public interface IReportService
{
    public Task<IReadOnlyList<LatestDeviceReportDto>> GetLatestDevicesReading(Guid userId,
        CancellationToken cancellationToken);
}
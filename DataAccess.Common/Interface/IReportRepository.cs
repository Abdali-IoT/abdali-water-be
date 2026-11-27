using DataAccess.Common.Entity;

namespace DataAccess.Common.Interface;

public interface IReportRepository
{
    public Task<IEnumerable<LatestDeviceReportEntity>> GetLatestDevicesReading(Guid userId,
        CancellationToken cancellationToken);
}
using Common.Dto;
using DataAccess.Common.Interface;
using Service.Interface;
using Common.Mapper;

namespace Service.Implementation;

public class ReportService(IReportRepository reportRepository) : IReportService
{
    public async Task<IReadOnlyList<LatestDeviceReportDto>> GetLatestDevicesReading(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var list = await reportRepository.GetLatestDevicesReading(userId, cancellationToken);
        return list.ToDtoList();
    }
    
}
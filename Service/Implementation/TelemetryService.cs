using DataAccess.Common.Interface;
using Service.Interface;

namespace Service.Implementation;

public class TelemetryService(ITelemetryRepository telemetryRepository) : ITelemetryService
{
    public async Task IngestTelemetryAsync(string payload, CancellationToken cancellationToken =  default)
    {
        await telemetryRepository.IngestTelemetryAsync(payload, cancellationToken);
    }
}
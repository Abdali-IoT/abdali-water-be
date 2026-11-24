using DataAccess.Common.Entity;

namespace DataAccess.Common.Interface;

public interface ITelemetryRepository
{
    public Task IngestTelemetryAsync(string payload, CancellationToken cancellationToken); 
}
namespace Service.Interface;

public interface ITelemetryService
{
    public Task IngestTelemetryAsync(string payload, CancellationToken cancellationToken); 
    
}
using System.Data;
using Dapper;
using DataAccess.Common.Entity;
using DataAccess.Common.Interface;

namespace DataAcess.Repository;

public class TelemetryRepository(Func<IDbConnection> connectionFactory) : ITelemetryRepository
{
    public async Task IngestTelemetryAsync(string payload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        string sql = """
                     SELECT
                        status,
                        stored_message_id,
                        inserted_readings
                     FROM ingest_telemetry(@payload);
                     """;

        using var connection = connectionFactory();
        await connection.QueryFirstOrDefaultAsync(sql, new { payload });
    }
}
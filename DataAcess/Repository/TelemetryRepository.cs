using System.Data;
using Dapper;

namespace DataAcess.Repository;

public class TelemetryRepository(Func<IDbConnection> connectionFactory) : ITelemetryRepository
{
    public async Task IngestTelemetryAsync(string payload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string sql = """
                     SELECT *
                     FROM public.ingest_telemetry(@Payload::jsonb);
                     """;

        using var connection = connectionFactory();

        var parameters = new DynamicParameters();
        parameters.Add("Payload", payload, DbType.String);

        var command = new CommandDefinition(
            commandText: sql,
            parameters: parameters,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        await connection.QuerySingleAsync(command);
    }
}
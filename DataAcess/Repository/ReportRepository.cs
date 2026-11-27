using System.Data;
using Dapper;
using DataAccess.Common.Entity;
using DataAccess.Common.Interface;

namespace DataAcess.Repository;

public class ReportRepository(Func<IDbConnection> connectionFactory) : IReportRepository
{
    public async Task<IEnumerable<LatestDeviceReportEntity>> GetLatestDevicesReading(Guid userId,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           select distinct on (t.measurement_type_id)
                               replace(d.device_type::text, '_', '') AS device_type, d.name as device_name,
                               t.device_id, t.measurement_code, t.measurement_name, t.unit,
                               t.value_kind, t.value_number, t.value_boolean, t.value_text,
                               t.measured_at, t.received_at
                           from public.vw_device_telemetry t
                           inner join public.user_devices ud ON ud.device_id = t.device_id
                           inner join public.devices d on ud.device_id = d.id
                           where ud.user_id = @userId
                           order by
                               t.measurement_type_id, t.measured_at desc, t.telemetry_message_id desc;
                           """;

        using var connection = connectionFactory();
        var list = await connection.QueryAsync<LatestDeviceReportEntity>(sql, new { userId });
        return list;
    }
}
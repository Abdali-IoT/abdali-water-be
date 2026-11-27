using Common.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Service.Interface;

namespace API.Endpoints;

internal static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var map = endpoints.MapGroup("/report/device");
        map.MapGet("/latest", GetLatestDeviceReport).RequireAuthorization();
    }

    private static async Task<Results<Ok<IReadOnlyList<LatestDeviceReportDto>>, BadRequest, UnauthorizedHttpResult>>
        GetLatestDeviceReport(
            IReportService reportService,
            HttpContext context,
            CancellationToken cancellationToken = default
        )
    {
        var user = context.Features.Get<UserDto>() ?? throw new ArgumentException("User not found");
        var list = await reportService.GetLatestDevicesReading(user.Id, cancellationToken);
        return TypedResults.Ok(list);
    }
}
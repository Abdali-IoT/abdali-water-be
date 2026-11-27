using API.Configuration;
using API.Endpoints;
using Dapper;
using DataAccess.Common.Enum;
using DataAccess.Common.Handler;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
try
{
    SqlMapper.AddTypeHandler(new StringEnumHandler<DeviceTypeEnum>());
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddApplicationServices(builder.Configuration);

    var app = builder.Build();
    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapAuthenticationEndpoints();
    app.MapMqttBrokerEndpoints();
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Error(ex, ex.Message);
}
finally
{
    await Log.CloseAndFlushAsync();
}
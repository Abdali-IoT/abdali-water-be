using API.Configuration;
using API.Endpoints;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
try
{
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
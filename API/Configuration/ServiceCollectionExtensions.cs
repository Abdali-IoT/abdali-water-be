using System.Data;
using System.Reflection;
using API.Authentication;
using API.Handlers;
using API.MQTT;
using API.Schedular;
using Dapper.FluentMap;
using Dapper.FluentMap.Naming;
using DataAccess.Common.Entity;
using DataAccess.Common.Interface;
using DataAcess.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using MQTTnet;
using Npgsql;
using Serilog;
using Service.Authentication;
using Service.Implementation;
using Service.Interface;

namespace API.Configuration;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var clientId = Environment.GetEnvironmentVariable("GOOGLE__CLIENT_ID") ??
                       throw new ArgumentException("GOOGLE__CLIENT_ID environment variable not set");

        var apiTokens = new ApiAccessTokens(
            configuration["Authentication:Jwt:Issuer"],
            configuration["Authentication:Jwt:Audience"],
            Environment.GetEnvironmentVariable("GOOGLE__SIGNING_KEY")
        );

        var allowedOrigins = configuration.GetSection("AllowedHosts").Get<string[]>() ?? [];

        FluentMapper.Initialize(config =>
        {
            // config.UseNamingPolicy(NamingPolicy.SnakeCase, caseSensitive: false)
            // .ForEntity<UserEntity>()
            // .ForEntity<DeviceEntity>()
            // ;
            config.UseNamingPolicy(NamingPolicy.SnakeCase, caseSensitive: false)
                .ForEntitiesInAssembly(Assembly.GetAssembly(typeof(DeviceEntity))!);
        });
        FluentMapper.Validate();

        services.AddSerilog((services, log) =>
        {
            log
                .Enrich.FromLogContext()
                .WriteTo.Console();
        });

        services.AddSingleton<Func<IDbConnection>>(_ => () =>
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION") ??
                                   throw new ArgumentException("DB_CONNECTION environment variable not set");
            return new NpgsqlConnection(connectionString);
        });

        services.AddSingleton(new GoogleCredentialVerifier(clientId));
        services.AddSingleton(apiTokens);
        services.AddSingleton<IMqttClient>(_ => new MqttClientFactory().CreateMqttClient());
        services.AddSingleton<MqttMessageDispatcher>();
        services.AddSingleton<MqttPublisher>();
        services.AddHostedService<MqttBackgroundService>();
        services.AddScoped<IAuthorizationHandler, EnabledUserAuthorizationHandler>();
        services.AddExceptionHandler<AuthenticationDependencyExceptionHandler>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMqttMessageHandler, TemperatureHandler>();
        services.AddScoped<IMqttMessageHandler, TelemetryHandler>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITelemetryRepository, TelemetryRepository>();
        services.AddScoped<ITelemetryService, TelemetryService>();

        services.AddProblemDetails();

        services.AddCors(options => options.AddDefaultPolicy(policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.IncludeErrorDetails = false;
                options.TokenValidationParameters = apiTokens.ValidationParameters;
            });

        var enabledUserPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new EnabledUserRequirement())
            .Build();

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(enabledUserPolicy)
            .SetFallbackPolicy(enabledUserPolicy);


        return services;
    }
}

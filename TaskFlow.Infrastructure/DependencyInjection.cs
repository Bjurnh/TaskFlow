using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Infrastructure.Caching;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Outbox;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Interceptors;

namespace TaskFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Registered as a singleton and injected into AddDbContext below via the service
        // provider callback - the interceptor itself is stateless, so one instance is fine
        // to share across all DbContext instances.
        services.AddSingleton<DispatchDomainEventsInterceptor>();

        services.AddDbContext<TaskFlowDbContext>((sp, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString("Default"))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<TaskFlowDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
        services.AddScoped<ICacheService, RedisCacheService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Application's AddApplication() already registered MediatR scanning ONLY the
        // Application assembly - it can't also scan Infrastructure, since Application must
        // never reference Infrastructure (that would invert the dependency direction Clean
        // Architecture depends on). Calling AddMediatR again here, scoped to THIS assembly,
        // is the standard way to add handlers that live in a different assembly: MediatR's
        // core services (IMediator/ISender/IPublisher) are registered with TryAdd internally,
        // so this call only ADDS the two EventHandlers below - it doesn't duplicate anything
        // already registered by Application.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // The background service that drains the Outbox - see OutboxProcessorService's own
        // comments for why it needs a scope factory rather than direct scoped dependencies.
        services.AddHostedService<OutboxProcessorService>();

        return services;
    }
}
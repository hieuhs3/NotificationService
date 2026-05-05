using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.MessageBus.Kafka;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Redis;
using NotificationService.Infrastructure.SignalR;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));
        
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddHostedService<KafkaConsumerService>();

        // We register SignalRSender here, but it's bound via CompositeSender in API
        services.AddScoped<SignalRNotificationSender>();

        return services;
    }
}

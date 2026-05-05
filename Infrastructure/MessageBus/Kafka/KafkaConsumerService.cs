using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Services;
using NotificationService.Domain.Events;

namespace NotificationService.Infrastructure.MessageBus.Kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<KafkaConsumerService> logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Auto-create topic if it doesn't exist
        var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        var topic = _configuration["Kafka:Topic"] ?? "notifications";

        try
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();
            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = topic,
                    ReplicationFactor = 1,
                    NumPartitions = 6 // Partition by userId for horizontal scaling
                }
            });
            _logger.LogInformation("Kafka topic '{Topic}' created with 6 partitions.", topic);
        }
        catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
        {
            _logger.LogInformation("Kafka topic '{Topic}' already exists.", topic);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not ensure Kafka topic exists. Will retry on consume.");
        }

        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }

    private async Task StartConsumerLoop(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"] ?? "notification-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false // Manual commit for at-least-once delivery
        };

        var topic = _configuration["Kafka:Topic"] ?? "notifications";

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
            .Build();

        consumer.Subscribe(topic);
        _logger.LogInformation("Kafka consumer started on topic: {Topic}", topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(2));
                    if (consumeResult == null) continue;

                    // Process message
                    try
                    {
                        var notificationEvent = JsonSerializer.Deserialize<NotificationEvent>(consumeResult.Message.Value);
                        if (notificationEvent != null)
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var handler = scope.ServiceProvider.GetRequiredService<NotificationHandler>();

                            await handler.HandleAsync(notificationEvent);

                            // Commit after successful processing (At-least-once)
                            consumer.Commit(consumeResult);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process message from topic {Topic}. Offset {Offset}", topic, consumeResult.Offset);
                        consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    // Topic not available yet — log and retry instead of crashing
                    _logger.LogWarning("Kafka consume error: {Reason}. Retrying in 5s...", ex.Error.Reason);
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer is stopping.");
        }
        finally
        {
            consumer.Close();
        }
    }
}

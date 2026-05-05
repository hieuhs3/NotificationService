using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Events;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Services;

public class NotificationHandler
{
    private readonly INotificationSender _sender;
    private readonly IIdempotencyService _idempotencyService;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<NotificationHandler> _logger;

    public NotificationHandler(
        INotificationSender sender,
        IIdempotencyService idempotencyService,
        INotificationRepository notificationRepository,
        ILogger<NotificationHandler> logger)
    {
        _sender = sender;
        _idempotencyService = idempotencyService;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task HandleAsync(NotificationEvent e)
    {
        var eventId = e.EventId.ToString();

        // 1. Check idempotency
        if (await _idempotencyService.HasBeenProcessedAsync(eventId))
        {
            _logger.LogWarning("Event {EventId} has already been processed. Skipping.", eventId);
            return;
        }

        // 2. Process
        try
        {
            _logger.LogInformation("[Instance: {MachineName}] Processing event {EventId} for user {UserId}", Environment.MachineName, eventId, e.UserId);
            
            // Send Notification
            await _sender.SendAsync(e);

            // 3. Mark as processed & Persistent Log
            await _idempotencyService.MarkAsProcessedAsync(eventId);

            var log = new NotificationLog
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = e.UserId,
                MessageBody = e.Message,
                Status = "Sent",
                Channel = e.Channel,
                Timestamp = DateTime.UtcNow
            };
            
            await _notificationRepository.AddLogAsync(log);

            _logger.LogInformation("Successfully processed and saved event {EventId}", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process event {EventId}", eventId);
            throw; // Let consumer retry or DLQ it
        }
    }
}

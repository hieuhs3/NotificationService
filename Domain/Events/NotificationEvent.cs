using System;

namespace NotificationService.Domain.Events;

public class NotificationEvent
{
    public string EventId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "General"; // e.g., "Alert", "General"
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Channel { get; set; } = "SignalR"; // e.g. "SignalR", "Email", "Push"
}

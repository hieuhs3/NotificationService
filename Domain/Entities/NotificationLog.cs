using System;

namespace NotificationService.Domain.Entities;

public class NotificationLog
{
    public Guid Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Sent", "Failed"
    public DateTime Timestamp { get; set; }
    public string MessageBody { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
}

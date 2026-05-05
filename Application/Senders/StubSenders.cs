using System.Threading.Tasks;
using NotificationService.Domain.Events;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Senders;

public class EmailSender : INotificationSender
{
    public Task SendAsync(NotificationEvent e)
    {
        if (e.Channel == "Email" || e.Channel == "All")
        {
            // Logic to send email
        }
        return Task.CompletedTask;
    }
}

public class PushSender : INotificationSender
{
    public Task SendAsync(NotificationEvent e)
    {
        if (e.Channel == "Push" || e.Channel == "All")
        {
            // Logic to send Push Notification
        }
        return Task.CompletedTask;
    }
}

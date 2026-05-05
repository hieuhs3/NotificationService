using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Events;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Infrastructure.SignalR;

public class SignalRNotificationSender : INotificationSender
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationSender(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendAsync(NotificationEvent e)
    {
        if (e.Channel == "SignalR" || e.Channel == "All")
        {
            // Send to a specific User via SignalR
            // Requires Authentication and UserId mapping to be configured
            await _hubContext.Clients.User(e.UserId).SendAsync("ReceiveNotification", e);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotificationService.Domain.Events;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Senders;

public class CompositeSender : INotificationSender
{
    private readonly IEnumerable<INotificationSender> _senders;

    public CompositeSender(IEnumerable<INotificationSender> senders)
    {
        _senders = senders;
    }

    public async Task SendAsync(NotificationEvent e)
    {
        foreach (var sender in _senders)
        {
            // Do not fail the whole process if one sender fails, but for now we just await.
            // Ideally we could try-catch here.
            try
            {
                await sender.SendAsync(e);
            }
            catch (Exception ex)
            {
                // In production, we'd log this exception
                Console.WriteLine($"Error sending via {sender.GetType().Name}: {ex.Message}");
            }
        }
    }
}

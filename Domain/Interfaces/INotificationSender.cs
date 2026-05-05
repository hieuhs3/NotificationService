using System.Threading.Tasks;
using NotificationService.Domain.Events;

namespace NotificationService.Domain.Interfaces;

public interface INotificationSender
{
    Task SendAsync(NotificationEvent e);
}

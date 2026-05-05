using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.SignalR;

public class NotificationHub : Hub
{
    // Clients will connect here and use JWT. We map UserId automatically if configured.
    // e.g. Context.UserIdentifier
}

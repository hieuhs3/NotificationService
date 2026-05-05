using System.Threading.Tasks;

namespace NotificationService.Application.Interfaces;

public interface IIdempotencyService
{
    Task<bool> HasBeenProcessedAsync(string eventId);
    Task MarkAsProcessedAsync(string eventId);
}

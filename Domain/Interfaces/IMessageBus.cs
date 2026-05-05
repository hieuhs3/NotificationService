using System.Threading;
using System.Threading.Tasks;

namespace NotificationService.Domain.Interfaces;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}

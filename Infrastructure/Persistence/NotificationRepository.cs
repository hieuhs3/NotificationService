using System.Threading.Tasks;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Infrastructure.Persistence;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddLogAsync(NotificationLog log)
    {
        _dbContext.NotificationLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }
}

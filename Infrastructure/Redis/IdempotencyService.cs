using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Redis;

public class IdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly TimeSpan _expireTime = TimeSpan.FromDays(7); 
    
    public IdempotencyService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> HasBeenProcessedAsync(string eventId)
    {
        var db = _redis.GetDatabase();
        var key = $"idempotency:{eventId}";
        return await db.KeyExistsAsync(key);
    }

    public async Task MarkAsProcessedAsync(string eventId)
    {
        var db = _redis.GetDatabase();
        var key = $"idempotency:{eventId}";
        await db.StringSetAsync(key, "processed", _expireTime);
    }
}

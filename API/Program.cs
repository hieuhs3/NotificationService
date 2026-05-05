using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Senders;
using NotificationService.Application.Services;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.SignalR;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// 1. Add Configuration
var configuration = builder.Configuration;

// 2. Add Infrastructure
builder.Services.AddInfrastructure(configuration);

// 3. Add SignalR with Redis Backplane for multi-instance support
builder.Services.AddSignalR()
    .AddStackExchangeRedis(configuration.GetConnectionString("Redis") ?? "localhost:6379", options => 
    {
        options.Configuration.ChannelPrefix = "NotificationService";
    });
builder.Services.AddSingleton<IUserIdProvider, QueryStringUserIdProvider>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.WithOrigins("null")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .SetIsOriginAllowed((host) => true)
                   .AllowCredentials();
        });
});

// 4. Add Senders & Domain Services
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<PushSender>();

// Register Composite Sender with all implementations
builder.Services.AddScoped<INotificationSender>(sp => 
{
    var senders = new List<INotificationSender>
    {
        sp.GetRequiredService<SignalRNotificationSender>(),
        sp.GetRequiredService<EmailSender>(),
        sp.GetRequiredService<PushSender>()
    };
    return new CompositeSender(senders);
});

// 5. Application Logic
builder.Services.AddScoped<NotificationHandler>();

var app = builder.Build();

// Run Migrations automatically for dev
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("AllowAll");
app.UseRouting();
app.MapHub<NotificationHub>("/hubs/notifications");

// API to fetch logs for the tester UI
app.MapGet("/api/logs", async (NotificationDbContext db) => 
{
    return await db.NotificationLogs
        .OrderByDescending(x => x.Timestamp)
        .Take(50)
        .ToListAsync();
});

app.Run();

public class QueryStringUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.GetHttpContext()?.Request.Query["userId"];
    }
}

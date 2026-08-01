using System.Text.Json;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Services;

public sealed class NotificationService(IApplicationDbContext dbContext) : INotificationService
{
    public async Task NotifyAsync(string userId, string type, string titleKey, object? payload, CancellationToken ct)
    {
        dbContext.Notifications.Add(Create(userId, type, titleKey, payload));
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task NotifyAdminsAsync(string type, string titleKey, object? payload, CancellationToken ct)
    {
        var adminIds = await dbContext.Users
            .Where(u => u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .ToListAsync(ct);

        foreach (var adminId in adminIds)
            dbContext.Notifications.Add(Create(adminId, type, titleKey, payload));

        await dbContext.SaveChangesAsync(ct);
    }

    private static Notification Create(string userId, string type, string titleKey, object? payload) => new()
    {
        UserId = userId,
        Type = type,
        Title = titleKey,
        Message = payload is null ? "{}" : JsonSerializer.Serialize(payload),
        CreatedAt = DateTime.Now
    };
}
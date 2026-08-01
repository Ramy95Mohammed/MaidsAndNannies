using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Notifications;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.WebApi.Services;

public sealed class BookingReminderService(
    IServiceScopeFactory scopeFactory,
    ILogger<BookingReminderService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDueBookingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "خطأ أثناء فحص حجوزات التذكير");
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CheckDueBookingsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var now = DateTime.UtcNow;

        var dueBookings = await dbContext.Bookings
            .Include(b => b.OriginalWorker)
            .ThenInclude(w=>w.User)
            .Include(b => b.Homeowner)
            .Where(b => b.ReminderSentAt == null
                && b.StartDate > now
                && b.StartDate <= now.AddDays(1)
                && b.Status != BookingStatus.Cancelled
                && b.Status != BookingStatus.Replaced)
            .ToListAsync(ct);

        foreach (var booking in dueBookings)
        {
            var payload = new
            {
                BookingId = booking.Id,
                StartDate = booking.StartDate,
                WorkerName = booking.OriginalWorker?.User.FullName ?? string.Empty,
                HomeownerName = booking.Homeowner.FullName
            };

            await notificationService.NotifyAsync(
                booking.HomeownerId, NotificationType.BookingReminder, "NOTIF.BOOKING_REMINDER", payload, ct);
            await notificationService.NotifyAsync(
                booking.WorkerId, NotificationType.BookingReminder, "NOTIF.BOOKING_REMINDER", payload, ct);

            booking.ReminderSentAt = now;
        }

        if (dueBookings.Count > 0)
            await dbContext.SaveChangesAsync(ct);
    }
}
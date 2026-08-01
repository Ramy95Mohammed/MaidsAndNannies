using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Notifications;
using MaidsAndNannies.Domain.Entities;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Reviews.Commands.ReviewBooking;

public sealed record ReviewBookingCommand(int BookingId, string ReviewerId, int Rating, string? Comment)
    : IRequest<Unit>;

public sealed class ReviewBookingCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notifications)
    : IRequestHandler<ReviewBookingCommand, Unit>
{
    public async Task<Unit> Handle(ReviewBookingCommand r, CancellationToken ct)
    {
        if (r.Rating is < 1 or > 5)
            throw new InvalidOperationException("التقييم يجب أن يكون بين 1 و 5");

        var booking = await dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == r.BookingId, ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.Status != BookingStatus.Completed)
            throw new InvalidOperationException("لا يمكن التقييم إلا بعد إتمام الحجز");

        var isHomeowner = booking.HomeownerId == r.ReviewerId;
        var isWorker = booking.WorkerId == r.ReviewerId;
        if (!isHomeowner && !isWorker)
            throw new UnauthorizedAccessException("غير مصرح لك بتقييم هذا الحجز");

        var alreadyReviewed = await dbContext.Reviews
            .AnyAsync(x => x.BookingId == r.BookingId && x.ReviewerId == r.ReviewerId, ct);
        if (alreadyReviewed)
            throw new InvalidOperationException("لقد قمت بتقييم هذا الحجز مسبقاً");

        var revieweeId = isHomeowner ? booking.WorkerId : booking.HomeownerId;

        dbContext.Reviews.Add(new Review
        {
            BookingId = r.BookingId,
            ReviewerId = r.ReviewerId,
            RevieweeId = revieweeId,
            Rating = r.Rating,
            Comment = r.Comment
        });

        // إعادة احتساب المتوسط والمجموع للمقيَّم (على كامل التقييمات المرئية)
        var stats = await dbContext.Reviews
            .Where(x => x.RevieweeId == revieweeId && x.IsVisible)
            .GroupBy(x => 1)
            .Select(g => new { Avg = (decimal)g.Average(x => x.Rating), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        var avg = stats?.Avg ?? r.Rating;
        var count = (stats?.Count ?? 0) + 1;

        var workerProfile = await dbContext.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == revieweeId, ct);
        if (workerProfile is not null)
        {
            workerProfile.AverageRating = avg;
            workerProfile.TotalReviews = count;
        }
        else
        {
            var homeownerProfile = await dbContext.HomeownerProfiles
                .FirstOrDefaultAsync(h => h.UserId == revieweeId, ct);
            if (homeownerProfile is not null)
            {
                homeownerProfile.AverageRating = avg;
                homeownerProfile.TotalReviews = count;
            }
        }

        await dbContext.SaveChangesAsync(ct);

        await notifications.NotifyAsync(revieweeId, NotificationType.NewReview, "NOTIF.NEW_REVIEW",
            new { BookingId = r.BookingId, Rating = r.Rating }, ct);

        return Unit.Value;
    }
}
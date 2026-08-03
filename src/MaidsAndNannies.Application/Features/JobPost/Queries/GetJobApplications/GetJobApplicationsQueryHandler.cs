using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.JobPosts.Common;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetJobApplications;

public sealed class GetJobApplicationsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetJobApplicationsQuery, IReadOnlyList<ApplicationDto>>
{
    public async Task<IReadOnlyList<ApplicationDto>> Handle(GetJobApplicationsQuery r, CancellationToken ct)
    {
        var isOwner = await dbContext.JobPosts.AnyAsync(j => j.Id == r.PostId && j.HomeownerId == r.HomeownerId, ct);
        if (!isOwner) throw new UnauthorizedAccessException("غير مصرح لك");

        // هل يوجد حجز نشط لنفس الإعلان؟ (حتى يُحذَّر صاحبة المنزل أن القبول لا يعدّل الحجز الحالي)
        var hasActiveBooking = await dbContext.Bookings.AnyAsync(
            b => b.JobPostId == r.PostId && b.HomeownerId == r.HomeownerId
                && b.Status != BookingStatus.Completed && b.Status != BookingStatus.Cancelled, ct);

        return await dbContext.JobApplications
            .Where(a => a.JobPostId == r.PostId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ApplicationDto(
                a.Id, a.WorkerId, a.Worker.FullName,
                a.Worker.WorkerProfile != null && a.Worker.WorkerProfile.Nationality != null
                    ? a.Worker.WorkerProfile.Nationality.Name_ar
                    : "",
                a.Worker.WorkerProfile != null ? a.Worker.WorkerProfile.AverageRating : 0,
                a.Worker.WorkerProfile != null ? a.Worker.WorkerProfile.TotalReviews : 0,
                a.Message, a.Status, a.CreatedAt, hasActiveBooking))
            .ToListAsync(ct);
    }
}
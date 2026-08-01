using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Notifications;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.ApplyForJob;

public sealed class ApplyForJobCommandHandler(
    IApplicationDbContext dbContext,
    INotificationService notifications)
    : IRequestHandler<ApplyForJobCommand, Unit>
{
    public async Task<Unit> Handle(ApplyForJobCommand r, CancellationToken ct)
    {
        var post = await dbContext.JobPosts
            .FirstOrDefaultAsync(j => j.Id == r.JobPostId && j.PostStatus == JobPostStatus.Approved, ct)
            ?? throw new KeyNotFoundException("الإعلان غير موجود أو غير معتمد");

        var alreadyAccepted = await dbContext.JobApplications
            .AnyAsync(a => a.JobPostId == r.JobPostId && a.WorkerId == r.WorkerId && a.Status == ApplicationStatus.Accepted, ct);
        if (alreadyAccepted)
            throw new InvalidOperationException("لديك طلب مقبول مسبقاً لهذا الإعلان");

        try
        {
            dbContext.JobApplications.Add(new JobApplication
            {
                JobPostId = r.JobPostId,
                WorkerId = r.WorkerId,
                Message = r.Message
            });
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException) when (ct.IsCancellationRequested == false)
        {
            // Unique constraint violation => duplicate
            throw new InvalidOperationException("لقد تقدمت لهذا الإعلان مسبقاً");
        }

        var workerName = await dbContext.Users
            .Where(u => u.Id == r.WorkerId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct) ?? "عاملة";

        await notifications.NotifyAsync(post.HomeownerId, NotificationType.NewApplication, "NOTIF.NEW_APPLICATION",
            new { WorkerId = r.WorkerId, WorkerName = workerName, PostId = post.Id, PostTitle = post.Description }, ct);

        return Unit.Value;
    }
}
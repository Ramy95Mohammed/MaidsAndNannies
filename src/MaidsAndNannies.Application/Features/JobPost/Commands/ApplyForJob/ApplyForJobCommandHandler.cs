using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.ApplyForJob;

public sealed class ApplyForJobCommandHandler(IApplicationDbContext dbContext)
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
        return Unit.Value;
    }
}
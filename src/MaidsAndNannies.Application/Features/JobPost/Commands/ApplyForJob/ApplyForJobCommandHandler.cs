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

        var already = await dbContext.JobApplications
            .AnyAsync(a => a.JobPostId == r.JobPostId && a.WorkerId == r.WorkerId, ct);
        if (already) throw new InvalidOperationException("لقد تقدمت لهذا الإعلان مسبقاً");

        dbContext.JobApplications.Add(new JobApplication
        {
            JobPostId = r.JobPostId,
            WorkerId = r.WorkerId,
            Message = r.Message
        });
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
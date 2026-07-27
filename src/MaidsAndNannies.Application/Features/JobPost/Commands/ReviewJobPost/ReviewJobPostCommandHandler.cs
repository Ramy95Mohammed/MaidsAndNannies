using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.ReviewJobPost;

public sealed class ReviewJobPostCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ReviewJobPostCommand, Unit>
{
    public async Task<Unit> Handle(ReviewJobPostCommand r, CancellationToken ct)
    {
        var post = await dbContext.JobPosts
            .FirstOrDefaultAsync(j => j.Id == r.PostId && j.PostStatus == JobPostStatus.Pending, ct)
            ?? throw new KeyNotFoundException("الإعلان غير موجود أو تمت مراجعته مسبقاً");

        post.SanitizedDescription = r.SanitizedDescription;
        post.PostStatus = r.IsApproved ? JobPostStatus.Approved : JobPostStatus.Rejected;
        post.RejectionReason = r.RejectionReason;
        post.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
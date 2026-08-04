using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.DeleteJobPost;

public sealed class DeleteJobPostCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteJobPostCommand, Unit>
{
    public async Task<Unit> Handle(DeleteJobPostCommand r, CancellationToken ct)
    {
        var post = await dbContext.JobPosts
            .Include(p => p.Applications)
            .FirstOrDefaultAsync(p => p.Id == r.PostId, ct)
            ?? throw new KeyNotFoundException("الإعلان غير موجود");

        if (post.HomeownerId != r.HomeownerId)
            throw new UnauthorizedAccessException("غير مصرح لك");

        if (post.Applications.Any())
            throw new InvalidOperationException("لا يمكن حذف الإعلان لوجود طلبات تقديم عليه");

        if (await dbContext.Bookings.AnyAsync(b => b.JobPostId == r.PostId, ct))
            throw new InvalidOperationException("لا يمكن حذف الإعلان لوجود حجوزات مرتبطة به");

        dbContext.JobPosts.Remove(post);
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
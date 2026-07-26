using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.CreateJobPost;

public sealed class CreateJobPostCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateJobPostCommand, int>
{
    public async Task<int> Handle(CreateJobPostCommand r, CancellationToken ct)
    {
        var post = new JobPost
        {
            HomeownerId = r.HomeownerId,
            Description = r.Description,
            MonthlySalary = r.MonthlySalary,
            DailySalary = r.DailySalary,
            HourlySalary = r.HourlySalary,
            Specialization = r.Specialization,
            BookingType = r.BookingType,
            CommissionType = r.CommissionType,
            StartDate = r.StartDate,
            Quantity = r.Quantity,
            CurrencyId = r.CurrencyId,
            PostStatus = JobPostStatus.Pending
        };
        dbContext.JobPosts.Add(post);
        await dbContext.SaveChangesAsync(ct);
        return post.Id;
    }
}
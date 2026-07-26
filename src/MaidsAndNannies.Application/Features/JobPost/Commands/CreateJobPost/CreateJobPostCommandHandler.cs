using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.CreateJobPost;

public sealed class CreateJobPostCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateJobPostCommand, int>
{
    public async Task<int> Handle(CreateJobPostCommand r, CancellationToken ct)
    {
        if (r.MonthlySalary <= 0 && r.DailySalary <= 0 && r.HourlySalary <= 0)
            throw new InvalidOperationException("يجب تحديد راتب واحد على الأقل");
        if (r.StartDate <= DateTime.UtcNow.Date)
            throw new InvalidOperationException("تاريخ البداية يجب أن يكون في المستقبل");
        if (r.Quantity < 1)
            throw new InvalidOperationException("العدد يجب أن يكون 1 على الأقل");

        var currencyExists = await dbContext.Currencies.AnyAsync(c => c.Id == r.CurrencyId, ct);
        if (!currencyExists)
            throw new KeyNotFoundException("العملة غير موجودة");

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
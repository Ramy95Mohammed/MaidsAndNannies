using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.CreateJobPost;

public sealed class CreateJobPostCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateJobPostCommand, int>
{
    public async Task<int> Handle(CreateJobPostCommand r, CancellationToken ct)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync();
        try
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

            var extras = (r.Specializations ?? new List<Specialization>())
                .Where(s => s != r.Specialization)
                .Distinct();
            foreach (var spec in extras)
                post.Specializations.Add(new JobPostSpecializationSpec { JobPostId = post.Id, JobSpecialization = spec });
            await dbContext.SaveChangesAsync(ct);

            await transaction.CommitAsync();

            return post.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
       
    }
}
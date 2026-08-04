using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.UpdateJobPost;

public sealed class UpdateJobPostCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateJobPostCommand, Unit>
{
    public async Task<Unit> Handle(UpdateJobPostCommand r, CancellationToken ct)
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

        var post = await dbContext.JobPosts
            .Include(p => p.Specializations)
            .FirstOrDefaultAsync(p => p.Id == r.PostId, ct)
            ?? throw new KeyNotFoundException("الإعلان غير موجود");

        if (post.HomeownerId != r.HomeownerId)
            throw new UnauthorizedAccessException("غير مصرح لك");

        post.Description = r.Description;
        post.MonthlySalary = r.MonthlySalary;
        post.DailySalary = r.DailySalary;
        post.HourlySalary = r.HourlySalary;
        post.Specialization = r.Specialization;
        post.BookingType = r.BookingType;
        post.CommissionType = r.CommissionType;
        post.StartDate = r.StartDate;
        post.Quantity = r.Quantity;
        post.CurrencyId = r.CurrencyId;
        post.PostStatus = JobPostStatus.Pending;
        post.RejectionReason = null;
        post.UpdatedAt = DateTime.UtcNow;

        post.Specializations.Clear();

        var extras = (r.Specializations ?? new List<Specialization>())
            .Where(s => s != r.Specialization)
            .Distinct();
        foreach (var spec in extras)
            post.Specializations.Add(new JobPostSpecializationSpec { JobPostId = post.Id, JobSpecialization = spec });

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
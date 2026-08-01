using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.JobPosts.Common;
using MaidsAndNannies.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetJobPostById;

public sealed class GetJobPostByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetJobPostByIdQuery, JobPostDetailDto>
{
    public async Task<JobPostDetailDto> Handle(GetJobPostByIdQuery r, CancellationToken ct)
    {
        var j = await dbContext.JobPosts.Include(p => p.Currency)
             .Include(p => p.Specializations)
            .FirstOrDefaultAsync(j => j.Id == r.Id, ct)
            ?? throw new KeyNotFoundException("الإعلان غير موجود");

        var isOwner = j.HomeownerId == r.UserId;
        var isAdmin = r.Role == "Admin";

        string? description;
        if (isOwner || isAdmin)
            description = j.Description;
        else if (j.PostStatus != JobPostStatus.Approved)
            description = "الإعلان غير متاح حالياً";
        else
            description = j.SanitizedDescription;

        return new JobPostDetailDto(
    j.Id, description,
    j.MonthlySalary, j.DailySalary, j.HourlySalary,
    j.Specialization, j.BookingType, j.CommissionType,
    j.StartDate, j.Quantity, j.PostStatus, j.RejectionReason,
    j.CreatedAt, isOwner, j.Currency.Code,
    j.Specializations.Select(s => s.JobSpecialization).ToList());
    }
}
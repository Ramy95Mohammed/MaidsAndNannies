using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Worker.Common;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Worker.Queries.GetWorkers;

public sealed class GetWorkersQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetWorkersQuery, PagedResult<WorkerSummaryDto>>
{
    public async Task<PagedResult<WorkerSummaryDto>> Handle(GetWorkersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.WorkerProfiles
            .Include(w => w.User)
            .Include(w => w.WorkerSpecializationSpecs)
            .Where(w => w.IsAvailable && w.VerificationStatus == VerificationStatus.Approved)
            .AsQueryable();

        if (request.CityId.HasValue)
            query = query.Where(w => w.CityId == request.CityId);

        if (request.Specialization.HasValue)
            query = query.Where(w => w.WorkerSpecializationSpecs != null &&
                w.WorkerSpecializationSpecs.Select(s => s.WorkerSpecialization).Contains(request.Specialization.Value));

        if (request.IsLiveIn.HasValue)
            query = query.Where(w => w.IsLiveIn == request.IsLiveIn.Value);

        if (request.MaxRate.HasValue)
            query = query.Where(w => w.MonthlyRate <= request.MaxRate.Value || w.HourlyRate <= request.MaxRate.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(w => w.User.FullName.Contains(request.Search) ||
                                      (w.Bio != null && w.Bio.Contains(request.Search)));

        if (request.NationalityId.HasValue)
            query = query.Where(w => w.NationalityId == request.NationalityId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var workers = await query
            .OrderByDescending(w => w.AverageRating)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WorkerSummaryDto(
                w.Id,
                w.User.FullName,
                w.NationalityId,
                w.WorkerSpecializationSpecs!.Select(s => new WorkerSpecializationDto(s.Id, s.WorkerSpecialization)).ToList(),
                w.IsLiveIn,
                w.MonthlyRate,
                w.HourlyRate,
                w.Currency,
                w.CityId,
                w.ExperienceYears,
                w.AverageRating,
                w.TotalReviews,
                w.User.ProfileImageUrl,
                w.Languages))
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkerSummaryDto>(workers, totalCount, request.Page, request.PageSize);
    }
}

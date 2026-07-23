using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Worker.Common;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Worker.Queries.GetWorkers;

public sealed class GetWorkersQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetWorkersQuery, PagedResult<WorkerSummaryDto>>
{
    public async Task<PagedResult<WorkerSummaryDto>> Handle(GetWorkersQuery request, CancellationToken ct)
    {
        var query = dbContext.WorkerProfiles
            .Include(w => w.User)
            .Include(w => w.WorkerSpecializationSpecs)
            .Where(w => w.VerificationStatus == VerificationStatus.Approved)
            .Where(w => w.IsAvailable)
            .AsQueryable();

        if (request.StateId.HasValue)
            query = query.Where(w => w.StateId == request.StateId);
        if (request.CityId.HasValue)
            query = query.Where(w => w.CityId == request.CityId);
        if (request.Specialization.HasValue)
            query = query.Where(w => w.WorkerSpecializationSpecs.Any(s => s.WorkerSpecialization == request.Specialization));
        if (request.IsLiveIn.HasValue)
            query = query.Where(w => w.IsLiveIn == request.IsLiveIn);
        if (request.MaxRate.HasValue)
            query = query.Where(w => w.MonthlyRate <= request.MaxRate);
        if (request.NationalityId.HasValue)
            query = query.Where(w => w.NationalityId == request.NationalityId);
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(w => w.User.FullName.Contains(request.Search) || (w.Bio != null && w.Bio.Contains(request.Search)));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(w => w.AverageRating)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WorkerSummaryDto(
                w.Id,
                w.User.FullName,
                w.NationalityId,
                w.WorkerSpecializationSpecs.Select(s => new WorkerSpecializationDto(s.Id, s.WorkerSpecialization)).ToList(),
                w.IsLiveIn,
                w.MonthlyRate,
                w.HourlyRate,
                w.Currency,
                w.CityId,
                w.ExperienceYears,
                w.AverageRating,
                w.TotalReviews,
                null,
                w.Languages))
            .ToListAsync(ct);

        return new PagedResult<WorkerSummaryDto>(items, total, request.Page, request.PageSize);
    }
}
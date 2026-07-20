using MaidsAndNannies.Application.Common.Exceptions;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Worker.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Worker.Queries.GetWorkerById;

public sealed class GetWorkerByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetWorkerByIdQuery, WorkerDetailDto>
{
    public async Task<WorkerDetailDto> Handle(GetWorkerByIdQuery request, CancellationToken cancellationToken)
    {
        var worker = await dbContext.WorkerProfiles
            .Include(w => w.User)
            .Include(w => w.Documents)
            .Include(w => w.WorkerSpecializationSpecs)
            .FirstOrDefaultAsync(w => w.Id == request.WorkerProfileId, cancellationToken);

        if (worker is null)
            throw new NotFoundException("WorkerProfile", request.WorkerProfileId);

        var reviews = await dbContext.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.RevieweeId == worker.UserId && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewSummaryDto(r.Id, r.Reviewer.FullName, r.Rating, r.Comment, r.CreatedAt))
            .ToListAsync(cancellationToken);

        return new WorkerDetailDto(
            worker.Id,
            worker.User.FullName,
            worker.NationalityId,
            worker.NationalIdNumber,
            worker.PassportNumber,
            worker.PassportCountry,
            worker.Bio,
            worker.ExperienceYears,
            (worker.WorkerSpecializationSpecs ?? [])
                .Select(s => new WorkerSpecializationDto(s.Id, s.WorkerSpecialization)).ToList(),
            worker.IsLiveIn,
            worker.MonthlyRate,
            worker.HourlyRate,
            worker.Currency,
            worker.CityId,
            worker.Address,
            worker.AverageRating,
            worker.TotalReviews,
            worker.IsAvailable,
            worker.User.ProfileImageUrl,
            worker.Languages,
            reviews);
    }
}

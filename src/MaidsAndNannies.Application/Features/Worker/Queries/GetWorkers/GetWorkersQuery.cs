using MaidsAndNannies.Application.Features.Worker.Common;
using MaidsPlatform.API.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.Worker.Queries.GetWorkers;

public sealed record GetWorkersQuery(
    int? CityId,
    Specialization? Specialization,
    bool? IsLiveIn,
    decimal? MaxRate,
    int? NationalityId,
    string? Search,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<WorkerSummaryDto>>;

using MaidsAndNannies.Application.Features.Worker.Common;
using MaidsPlatform.API.Domain.Enums;
using MediatR;

public sealed record GetWorkersQuery(
    int? StateId,
    int? CityId,
    Specialization? Specialization,
    bool? IsLiveIn,
    decimal? MaxRate,
    int? NationalityId,
    string? Search,
    int Page,
    int PageSize,
    string? UserId,
    string? Role) : IRequest<PagedResult<WorkerSummaryDto>>;
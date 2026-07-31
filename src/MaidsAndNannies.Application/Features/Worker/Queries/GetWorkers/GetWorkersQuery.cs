using MaidsAndNannies.Application.Features.Worker.Common;
using MaidsPlatform.API.Domain.Enums;
using MediatR;

public sealed record GetWorkersQuery(
    int? StateId,
    int? CityId,
    List<Specialization>? Specializations,
    bool? IsLiveIn,
    decimal? MaxRate,
    int? currencyId,
    int? NationalityId,
    string? Search,
    int Page,
    int PageSize,
    string? UserId,
    string? Role) : IRequest<PagedResult<WorkerSummaryDto>>;
using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAllHomeowners;

public sealed record GetAllHomeownersQuery() : IRequest<IReadOnlyList<MaidsAndNannies.Application.Features.Admin.Common.AdminHomeownerDto>>;
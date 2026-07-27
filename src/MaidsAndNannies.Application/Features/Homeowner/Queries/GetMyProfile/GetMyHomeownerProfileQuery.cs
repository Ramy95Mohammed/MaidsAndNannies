using MediatR;
using MaidsAndNannies.Application.Features.Homeowner.Common;

namespace MaidsAndNannies.Application.Features.Homeowner.Queries.GetMyProfile;

public sealed record GetMyHomeownerProfileQuery(string UserId) : IRequest<HomeownerProfileDto>;
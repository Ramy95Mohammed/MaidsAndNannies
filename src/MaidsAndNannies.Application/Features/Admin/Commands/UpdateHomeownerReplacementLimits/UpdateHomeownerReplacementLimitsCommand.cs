using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Commands.UpdateHomeownerReplacementLimits;

public sealed record UpdateHomeownerReplacementLimitsCommand(
    int HomeownerProfileId,
    int? MaxFaultReplacementCount,
    int? MaxPreferenceReplacementCount) : IRequest<Unit>;
using MediatR;
using MaidsAndNannies.Application.Features.AdminSettings.Common;

namespace MaidsAndNannies.Application.Features.AdminSettings.Queries.GetSettings;

public sealed record GetSettingsQuery : IRequest<IReadOnlyList<SettingDto>>;
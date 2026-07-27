using MediatR;

namespace MaidsAndNannies.Application.Features.AdminSettings.Commands.UpdateSettings;

public sealed record UpdateSettingsCommand(
    IReadOnlyList<UpdateSettingItem> Settings,
    string UpdatedBy) : IRequest<Unit>;

public sealed record UpdateSettingItem(string Key, string Value);
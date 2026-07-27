namespace MaidsAndNannies.Application.Features.AdminSettings.Common;

public sealed record SettingDto(string Key, string Value, string? Description);

public sealed record UpdateSettingsRequest(string Key, string Value);
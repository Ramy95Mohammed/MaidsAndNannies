using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Application.Features.Auth.Common;

public enum AccountType { Homeowner, Worker }

public sealed record AuthResponseDto(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string FullName,
    string Role,
    string PreferredLanguage,
    VerificationStatus VerificationStatus);

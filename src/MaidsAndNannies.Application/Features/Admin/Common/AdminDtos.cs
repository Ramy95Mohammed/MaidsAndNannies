using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Application.Features.Admin.Common;

public sealed record PendingWorkerDto(
    int Id,
    string UserId,
    string FullName,
    string? Email,
    string? PhoneNumber,
    int NationalityId,
    string NationalIdNumber,
    string? PassportNumber,
    int ExperienceYears,
    VerificationStatus VerificationStatus,
    DateTime CreatedAt,
    IReadOnlyList<PendingDocumentDto> Documents);

public sealed record PendingDocumentDto(string Type, string? DocumentImageUrl);

public sealed record PendingHomeownerDto(
    int Id,
    string UserId,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string City,
    string Address,
    VerificationStatus VerificationStatus,
    DateTime CreatedAt);

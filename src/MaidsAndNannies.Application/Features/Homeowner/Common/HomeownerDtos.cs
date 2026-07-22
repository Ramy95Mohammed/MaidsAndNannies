using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Application.Features.Homeowner.Common;

public sealed record HomeownerProfileDto(
    int Id,
    string UserId,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string WhatsAppNumber,
    string NationalIdNumber,
    string? NationalIdImageUrl,
    string? SelfieImageUrl,
    string? ProofOfAddressImageUrl,
    string Address,
    string State,
    string City,
    string? District,
    VerificationStatus VerificationStatus,
    string? VerificationNotes,
    DateTime? VerifiedAt);

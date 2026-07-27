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

public sealed record AdminDashboardDto(
    int TotalUsers,
    int TotalHomeowners,
    int TotalWorkers,
    int TotalBookings,
    int ActiveBookings,
    int PendingVerifications,
    int PendingPayments);

public sealed record PendingPaymentDto(
    int Id,
    int BookingId,
    string HomeownerName,
    decimal Amount,
    decimal CommissionAmount,
    PaymentMethod PaymentMethod,
    string? TransactionReference,
    string? ProofImageUrl,
    bool IsConfirmed,
    DateTime CreatedAt);
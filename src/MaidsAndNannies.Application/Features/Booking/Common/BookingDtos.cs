using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Application.Features.Bookings.Common;

public sealed record BookingListDto(
    int Id,
    string WorkerName,
    int WorkerId,
    Specialization ServiceType,
    DateTime StartDate,
    decimal MonthlySalary,
    decimal CommissionAmount,
    BookingStatus Status,
    bool IsPaid,
    int ReplacementCount,
    DateTime CreatedAt);

public sealed record AdminBookingListDto(
    int Id,
    string WorkerName,
    string HomeownerName,
    int WorkerId,
    Specialization ServiceType,
    DateTime StartDate,
    decimal MonthlySalary,
    decimal CommissionAmount,
    BookingStatus Status,
    bool IsPaid,
    int ReplacementCount,
    DateTime CreatedAt);

public sealed record BookingDetailDto(
    int Id,
    string HomeownerId,
    string HomeownerName,
    string? HomeownerPhone,
    string WorkerId,
    string? WorkerFullName,
    string? WorkerPhone,
    string? WorkerWhatsApp,
    string? WorkerProfileImage,
    int? WorkerNationalityId,
    Specialization ServiceType,
    DateTime StartDate,
    DateTime? EndDate,
    decimal MonthlySalary,
    decimal CommissionAmount,
    CommissionType CommissionType,
    BookingStatus Status,
    bool IsPaid,
    int ReplacementCount,
    string? AdminNotes,
    DateTime CreatedAt);

public sealed record CreateBookingRequest(
    int WorkerId,
    Specialization ServiceType,
    DateTime StartDate,
    decimal MonthlySalary,
    CommissionType CommissionType);

public sealed record UploadPaymentProofRequest(
    PaymentMethod PaymentMethod,
    decimal Amount,
    decimal CommissionAmount,
    string? TransactionReference);
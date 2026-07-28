using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Application.Features.Bookings.Common;

public sealed record BookingListDto(
    int Id,
    string WorkerName,
    int WorkerId,
    Specialization ServiceType,
    BookingType BookingType,
    int Quantity,
    string CurrencyCode,
    DateTime StartDate,
    decimal MonthlySalary,
    decimal DailySalary,
    decimal HourlySalary,
    decimal TotalAmount,
    decimal TotalAmountAfterConversion,
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
    BookingType BookingType,
    int Quantity,
    string CurrencyCode,
    DateTime StartDate,
    decimal MonthlySalary,
      decimal DailySalary,
    decimal HourlySalary,
    decimal TotalAmount,
    decimal TotalAmountAfterConversion,
    decimal CommissionAmount,
    BookingStatus Status,
    bool IsPaid,
    int ReplacementCount,
    int MaxReplacement,
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
    BookingType BookingType,
    int Quantity,
    string CurrencyCode,
    DateTime StartDate,
    DateTime? EndDate,
    decimal MonthlySalary,
    decimal DailySalary,
    decimal HourlySalary,
    decimal TotalAmount,
    decimal TotalAmountAfterConversion,
    decimal CommissionAmount,
    CommissionType CommissionType,
    BookingStatus Status,
    bool IsPaid,
    int ReplacementCount,
    int MaxReplacement,
    string? AdminNotes,
    DateTime CreatedAt,
    int? JobPostId,
    decimal OutstandingAmount,
    int MaxFaultReplacement,
    int MaxPreferenceReplacement);


public sealed record CreateBookingRequest(
    int WorkerId,
    Specialization ServiceType,
    BookingType BookingType,
    int Quantity,
    DateTime StartDate,
    decimal MonthlySalary,
    decimal DailySalary,
    decimal HourlySalary,
    CommissionType CommissionType);

public sealed record UploadPaymentProofRequest(
    PaymentMethod PaymentMethod,
    decimal Amount,
    decimal CommissionAmount,
    string? TransactionReference);

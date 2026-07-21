using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Application.Features.Worker.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Data, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed record WorkerSpecializationDto(int Id, Specialization WorkerSpecialization);

public sealed record WorkerSummaryDto(
    int Id,
    string FullName,
    int NationalityId,
    IReadOnlyList<WorkerSpecializationDto> WorkerSpecializationSpecs,
    bool IsLiveIn,
    decimal? MonthlyRate,
    decimal? HourlyRate,
    int Currency,
    int? CityId,
    int ExperienceYears,
    decimal AverageRating,
    int TotalReviews,
    string? ProfileImageUrl,
    string? Languages);

public sealed record ReviewSummaryDto(int Id, string ReviewerName, int Rating, string? Comment, DateTime CreatedAt);

public sealed record WorkerDetailDto(
    int Id,
    string FullName,
    int NationalityId,
    string NationalIdNumber,
    string? PassportNumber,
    string? PassportCountry,
    string? Bio,
    int ExperienceYears,
    IReadOnlyList<WorkerSpecializationDto> WorkerSpecializationSpecs,
    bool IsLiveIn,
    decimal? MonthlyRate,
    decimal? HourlyRate,
    int Currency,
    int? CityId,
    string? Address,
    decimal AverageRating,
    int TotalReviews,
    bool IsAvailable,
    string? ProfileImageUrl,
    string? Languages,
    IReadOnlyList<ReviewSummaryDto> Reviews);

public sealed record WorkerDocumentDto(DocumentType Type, string? DocumentImageUrl, VerificationStatus VerificationStatus);

public sealed record WorkerProfileDto(
    int Id,
    string UserId,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? WhatsAppNumber,
    int NationalityId,
    string NationalIdNumber,
    string? PassportNumber,
    DateTime? PassportExpiryDate,
    string? PassportCountry,
    int? CountryId,
    int? StateId,
    int? CityId,
    string? Address,
    string? Bio,
    int ExperienceYears,
    string? PreviousEmployer,
    IReadOnlyList<WorkerSpecializationDto> WorkerSpecializationSpecs,
    bool IsLiveIn,
    bool IsAvailable,
    decimal? MonthlyRate,
    decimal? HourlyRate,
    int Currency,
    decimal AverageRating,
    int TotalReviews,
    string? Languages,
    VerificationStatus VerificationStatus,
    string? VerificationNotes,
    DateTime? VerifiedAt,
    string? VerifiedBy,
    IReadOnlyList<WorkerDocumentDto> Documents);

public sealed record MyBookingDto(
    int Id,
    string HomeownerName,
    Specialization ServiceType,
    DateTime StartDate,
    DateTime? EndDate,
    decimal MonthlySalary,
    decimal CommissionAmount,
    CommissionType CommissionType,
    BookingStatus Status,
    bool IsPaid,
    DateTime CreatedAt);

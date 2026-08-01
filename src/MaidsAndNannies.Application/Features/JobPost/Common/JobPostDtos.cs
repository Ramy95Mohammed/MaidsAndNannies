using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Application.Features.JobPosts.Common;

public sealed record JobPostListDto(
    int Id, string Description, decimal MonthlySalary, decimal DailySalary,
    decimal HourlySalary, Specialization Specialization, BookingType BookingType,
    CommissionType CommissionType, DateTime StartDate, int Quantity,
    JobPostStatus PostStatus, string? RejectionReason, DateTime CreatedAt,
    int ApplicationCount, string CurrencyCode,
    IReadOnlyList<Specialization>? Specializations = null);

public sealed record JobPostDetailDto(
    int Id, string? Description, decimal MonthlySalary, decimal DailySalary,
    decimal HourlySalary, Specialization Specialization, BookingType BookingType,
    CommissionType CommissionType, DateTime StartDate, int Quantity,
    JobPostStatus PostStatus, string? RejectionReason, DateTime CreatedAt, bool IsOwner, string CurrencyCode,
    IReadOnlyList<Specialization>? Specializations = null);

public sealed record ApplicationDto(
    int Id, string WorkerId, string WorkerName, string? WorkerNationality,
    decimal WorkerRating, int WorkerReviews, string? Message,
    ApplicationStatus Status, DateTime CreatedAt, bool HasActiveBookingForPost = false);
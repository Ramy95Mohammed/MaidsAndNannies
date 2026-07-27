using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetPendingJobPosts;

public sealed record GetPendingJobPostsQuery : IRequest<IReadOnlyList<PendingJobPostDto>>;

public sealed record PendingJobPostDto(
    int Id, string HomeownerName, string Description,
    decimal MonthlySalary, decimal DailySalary, decimal HourlySalary,
    int BookingType, int CommissionType, int Specialization,
    DateTime StartDate, int Quantity, DateTime CreatedAt , string CurrencyCode);
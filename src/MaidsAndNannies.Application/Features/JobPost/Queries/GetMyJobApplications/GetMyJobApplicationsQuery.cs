using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetMyJobApplications;

public sealed record GetMyJobApplicationsQuery(string WorkerId) : IRequest<IReadOnlyList<MyApplicationDto>>;

public sealed record MyApplicationDto(
    int Id, int JobPostId, string? Message,
    ApplicationStatus Status, DateTime CreatedAt,
    decimal PostMonthlySalary, BookingType PostBookingType,
    JobPostStatus PostStatus);
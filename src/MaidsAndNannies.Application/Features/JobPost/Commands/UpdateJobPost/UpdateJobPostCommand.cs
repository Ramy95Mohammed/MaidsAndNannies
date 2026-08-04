using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.UpdateJobPost;

public sealed record UpdateJobPostCommand(
    int PostId, string HomeownerId, string Description, decimal MonthlySalary,
    decimal DailySalary, decimal HourlySalary, Specialization Specialization,
    BookingType BookingType, CommissionType CommissionType,
    DateTime StartDate, int Quantity, int CurrencyId,
    List<Specialization>? Specializations = null) : IRequest<Unit>;
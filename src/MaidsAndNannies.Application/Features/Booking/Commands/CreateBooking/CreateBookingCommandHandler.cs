using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateBookingCommand, int>
{
    public async Task<int> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        var worker = await dbContext.WorkerProfiles
            .FirstOrDefaultAsync(w => w.Id == request.WorkerId, ct)
            ?? throw new KeyNotFoundException("العاملة غير موجودة");

        var commissionAmount = request.MonthlySalary * 0.1m;

        var booking = new Booking
        {
            HomeownerId = request.HomeownerId,
            WorkerId = worker.UserId,
            OriginalWorkerId = worker.Id,
            ServiceType = request.ServiceType,
            StartDate = request.StartDate,
            MonthlySalary = request.MonthlySalary,
            CommissionAmount = commissionAmount,
            CommissionType = request.CommissionType,
            Status = BookingStatus.Pending,
            ReplacementCount = 0
        };

        dbContext.Bookings.Add(booking);

        if (request.CommissionType == CommissionType.Subscription)
        {
            dbContext.Subscriptions.Add(new Domain.Entities.Subscription
            {
                HomeownerId = request.HomeownerId,
                PlanType = CommissionType.Subscription,
                Amount = commissionAmount,
                StartDate = request.StartDate,
                EndDate = request.StartDate.AddDays(30),
                IsActive = true
            });
        }

        await dbContext.SaveChangesAsync(ct);
        return booking.Id;
    }
}
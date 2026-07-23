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
        try
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
                CommissionType = CommissionType.OneTime,
                Status = BookingStatus.Pending,
                ReplacementCount = 0
            };

            dbContext.Bookings.Add(booking);
            await dbContext.SaveChangesAsync(ct);
            return booking.Id;
        }
        catch (Exception ex)
        {
            throw;
        }
      
    }
}
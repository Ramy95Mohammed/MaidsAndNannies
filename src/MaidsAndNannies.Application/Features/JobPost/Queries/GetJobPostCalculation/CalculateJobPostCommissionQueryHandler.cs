using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Booking.Common;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaidsAndNannies.Application.Features.JobPost.Queries.GetJobPostCalculation
{
    public class CalculateJobPostCommissionQueryHandler(ICalculateBookingCommissionData calculateBookingCommissionData , IApplicationDbContext dbContext)
        : IRequestHandler<CalculateJobPostCommissionQuery, BookingDetailDto>
    {
        public async Task<BookingDetailDto> Handle(CalculateJobPostCommissionQuery request, CancellationToken ct)
        {
            var currency = await dbContext.Currencies
               .FirstOrDefaultAsync(c => c.Id == request.CurrencyId, ct)
               ?? throw new KeyNotFoundException("العملة غير موجودة");

            var workerCalculationsVars = new HomeownerOrWorkerCalculationsVars();

            workerCalculationsVars.DailyRate = request.DailySalary;
            workerCalculationsVars.HourlyRate = request.HourlySalary;
            workerCalculationsVars.MonthlyRate = request.MonthlySalary;

            var bookingRequest = new BookingOrJobPostRequestVars
            {
                BookingType = request.BookingType,
                CommissionType = request.CommissionType,
                MonthlySalary = request.MonthlySalary,
                Quantity = request.Quantity,
            };

            var bookingCalculationsReturnValue = await calculateBookingCommissionData.Calc(bookingRequest, workerCalculationsVars, currency, ct);

            return new BookingDetailDto(0, "", "", null, "", "", null, null, null,
              null, Specialization.Childcare, BookingType.Daily, 0, "", DateTime.Now, null,
              0, 0, 0, bookingCalculationsReturnValue.TotalAmount, bookingCalculationsReturnValue.TotalInEgp,
              bookingCalculationsReturnValue.CommissionAmount, CommissionType.OneTime, BookingStatus.Pending,
              false, 0, 0, null, DateTime.Now, null, 0, 0, 0, bookingCalculationsReturnValue.PaymentAmount, true, false);                        
        }
    }
}

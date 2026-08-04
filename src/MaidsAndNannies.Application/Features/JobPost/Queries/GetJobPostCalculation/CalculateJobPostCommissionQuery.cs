using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaidsAndNannies.Application.Features.JobPost.Queries.GetJobPostCalculation
{
    public sealed record CalculateJobPostCommissionQuery(BookingType BookingType, int Quantity, CommissionType CommissionType,
        decimal MonthlySalary ,decimal DailySalary ,decimal HourlySalary, int CurrencyId) : IRequest<BookingDetailDto>
    {

    }
}

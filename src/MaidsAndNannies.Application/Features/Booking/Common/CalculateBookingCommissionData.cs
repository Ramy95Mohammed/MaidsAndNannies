using MaidsAndNannies.Application.Common.Interfaces;
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

namespace MaidsAndNannies.Application.Features.Booking.Common
{
    public class HomeownerOrWorkerCalculationsVars
    {
        public decimal? DailyRate { get; set; }
        public decimal? MonthlyRate { get; set; }
        public decimal? HourlyRate { get;set; }
    }

    public class BookingOrJobPostRequestVars
    {
        public int Quantity { get; set; }
        public decimal MonthlySalary { get; set; }
        public BookingType BookingType { get; set; }
        public CommissionType CommissionType { get; set; }
    }

     public class CalculationsReturnValue
    {
        public decimal TotalAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public CommissionType CommissionType { get; set; }
        public decimal TotalInEgp { get; set; }
        public decimal PaymentAmount { get; set; }
    }

    public interface ICalculateBookingCommissionData
    {
        Task<CalculationsReturnValue> Calc(BookingOrJobPostRequestVars request, HomeownerOrWorkerCalculationsVars  homeownerOrWorkerCalculationsVars,
           Domain.Entities.Currency currency
           , CancellationToken ct);
    }
    public class CalculateBookingCommissionData : ICalculateBookingCommissionData
    {
        private readonly IApplicationDbContext _dbContext;
        public CalculateBookingCommissionData(IApplicationDbContext dbContext)
        {                
            _dbContext = dbContext;
        }
        public async Task<CalculationsReturnValue> Calc(BookingOrJobPostRequestVars request,
            HomeownerOrWorkerCalculationsVars homeownerOrWorkerCalculationsVars,
            Domain.Entities.Currency currency
            , CancellationToken ct)
        {
            var settings = await _dbContext.AppSettings.ToListAsync(ct);

            var getPercent = (string key, int fallback) =>
            {
                var val = settings.FirstOrDefault(s => s.Key == key)?.Value;
                return int.TryParse(val, out var p) ? p : fallback;
            };

            var monthlyWorkingDays = getPercent("MonthlyWorkingDaysPerMonth", 26);
            if (monthlyWorkingDays < 1) monthlyWorkingDays = 26;

            var monthlyTotal = request.Quantity > 0
               ? request.MonthlySalary / monthlyWorkingDays * request.Quantity
               : request.MonthlySalary;

            var commissionPercent = request.BookingType switch
            {
                BookingType.Daily => getPercent("CommissionDailyPercent", 10),
                BookingType.Hourly => getPercent("CommissionHourlyPercent", 10),
                BookingType.Monthly => request.CommissionType == CommissionType.OneTime
                    ? getPercent("CommissionMonthlyOneTimePercent", 10)
                    : getPercent("CommissionMonthlySubscriptionPercent", 10),
                _ => 10
            };

            decimal totalAmount = request.BookingType switch
            {
                BookingType.Daily => (homeownerOrWorkerCalculationsVars.DailyRate ?? 0) * request.Quantity,
                BookingType.Hourly => (homeownerOrWorkerCalculationsVars.HourlyRate ?? 0) * request.Quantity,
                BookingType.Monthly => monthlyTotal,
                _ => monthlyTotal
            };

            var totalInEgp = totalAmount * currency.RateToEgp;
            var commissionAmount = totalInEgp * commissionPercent / 100m;

            // المبلغ الإجمالي المطلوب عند الدفع حسب الإعداد
            var billingMode = settings.FirstOrDefault(s => s.Key == "CommissionBillingMode")?.Value ?? "CommissionOnly";

            var workerFirstSalaryInEgp = request.BookingType switch
            {
                BookingType.Daily => (homeownerOrWorkerCalculationsVars.DailyRate ?? 0) * request.Quantity * currency.RateToEgp,
                BookingType.Hourly => (homeownerOrWorkerCalculationsVars.HourlyRate ?? 0) * request.Quantity * currency.RateToEgp,
                _ => monthlyTotal * currency.RateToEgp
            };

            var paymentAmount = billingMode == "CommissionPlusSalary"
                ? commissionAmount + workerFirstSalaryInEgp
                : commissionAmount;

            var commissionType = request.BookingType switch
            {
                BookingType.Monthly => request.CommissionType,
                _ => CommissionType.OneTime
            };


            return new CalculationsReturnValue { CommissionAmount = commissionAmount , CommissionType = commissionType 
                , PaymentAmount = paymentAmount , TotalAmount = totalAmount , TotalInEgp = totalInEgp
            };
        }
    }
}

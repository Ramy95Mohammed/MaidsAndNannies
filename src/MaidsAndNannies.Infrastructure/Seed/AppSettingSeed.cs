using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaidsAndNannies.Infrastructure.Seed
{
    public static class AppSettingSeed
    {
        public static async Task SeedAsync(IApplicationDbContext dbContext)
        {
            if (! await dbContext.AppSettings.AnyAsync())
            {
                await dbContext.AppSettings.AddRangeAsync(new List<AppSetting>
                {
                     new AppSetting { Key = "MaxFaultReplacementCount", Value = "1", Description = "الحد الأقصى لعدد مرات الاستبدال بسبب تقصير العاملة" },
                     new AppSetting { Key = "MaxPreferenceReplacementCount", Value = "1", Description = "الحد الأقصى لعدد مرات الاستبدال برغبة شخصية من صاحبة المنزل" },
                     new AppSetting { Key = "CommissionDailyPercent", Value = "10", Description = "نسبة العمولة للحجوزات اليومية (%)" },
                     new AppSetting { Key = "CommissionHourlyPercent", Value = "10", Description = "نسبة العمولة للحجوزات بالساعة (%)" },
                     new AppSetting { Key = "CommissionMonthlyOneTimePercent", Value = "10", Description = "نسبة العمولة للحجوزات الشهرية (مرة واحدة)" },
                     new AppSetting { Key = "CommissionMonthlySubscriptionPercent", Value = "10", Description = "نسبة العمولة للحجوزات الشهرية (اشتراك شهري)" },
                     new AppSetting { Key = "AutoCancelPendingBookingHours", Value = "48", Description = "إلغاء الحجوزات المعلقة تلقائياً بعد (ساعة)" },
                     new AppSetting { Key = "MaxActiveBookingsPerHomeowner", Value = "5", Description = "الحد الأقصى للحجوزات النشطة لكل صاحبة منزل" },
                      new AppSetting { Key = "CommissionBillingMode", Value = "CommissionOnly", Description = "المبلغ المطلوب من صاحبة المنزل عند الدفع: CommissionOnly = العمولة فقط، CommissionPlusSalary = العمولة + مرتب العاملة" },
                     new AppSetting { Key = "RequirePaymentProof", Value = "true", Description = "إظهار قسم رفع إثبات الدفع: true = ترفع صاحبة المنزل إثبات الدفع، false = يُعتبر الحجز مدفوعاً فور طلب الدفع (التواصل عبر واتساب)"},
                     new AppSetting
                     {
                         Key = "MonthlyWorkingDaysPerMonth",
                         Value = "26",
                         Description = "عدد أيام العمل القياسية في الشهر لحساب الأجر الشهري النسبي"
                     },
                      new AppSetting
                     {
                         Key = "ShowDbRestoreSection",
                         Value = "false",
                         Description = "اظهار / اخفاء استعادة قاعدة البيانات"
                     }
                  });

                await dbContext.SaveChangesAsync();
            }
        }
    }
}

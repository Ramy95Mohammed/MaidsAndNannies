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
    public static class CurrenciesSeed
    {
        public static async Task SeedAsync(IApplicationDbContext dbContext)
        {
            if (!await dbContext.Currencies.AnyAsync())
            {
                await dbContext.Currencies.AddRangeAsync(new List<Currency>
            {
                new Currency {  Code = "EGP", Symbol = "E£", NameAr = "جنيه مصري", NameEn = "Egyptian Pound", RateToEgp = 1m, IsActive = true },
                new Currency {  Code = "USD", Symbol = "$", NameAr = "دولار أمريكي", NameEn = "US Dollar", RateToEgp = 48.5m, IsActive = true },
                new Currency {  Code = "SAR", Symbol = "﷼", NameAr = "ريال سعودي", NameEn = "Saudi Riyal", RateToEgp = 12.9m, IsActive = true }
            });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}

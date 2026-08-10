using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaidsAndNannies.Infrastructure.Seed
{
    public static class UsersSeed
    {
        public static async Task SeedAOnDevelopsync(UserManager<ApplicationUser> userManager , IApplicationDbContext dbContext)
        {
            // ── Seed 3 Homeowners ──
            var homeowners = new[]
            {
            new { Email = "homeowner1@maidsandnannies.local", Name = "سارة أحمد", Phone = "01110000001" },
            new { Email = "homeowner2@maidsandnannies.local", Name = "نورة خالد", Phone = "01110000002" },
            new { Email = "homeowner3@maidsandnannies.local", Name = "مريم عمر", Phone = "01110000003" },
        };

            foreach (var h in homeowners)
            {
                if (await userManager.FindByEmailAsync(h.Email) is not null) continue;

                var user = new ApplicationUser
                {
                    UserName = h.Email,
                    Email = h.Email,
                    FullName = h.Name,
                    PreferredLanguage = "ar",
                    Role = UserRole.Homeowner,
                    EmailConfirmed = true,
                    PhoneNumber = h.Phone
                };

                var result = await userManager.CreateAsync(user, "Homeowner@12345");
                if (!result.Succeeded) continue;

                await userManager.AddToRoleAsync(user, UserRole.Homeowner.ToString());

                dbContext.HomeownerProfiles.Add(new HomeownerProfile
                {
                    UserId = user.Id,
                    Address = $"عنوان {h.Name}",
                    City = "القاهرة",
                    State = "مصر",
                    WhatsAppNumber = h.Phone,
                    VerificationStatus = VerificationStatus.Pending,
                    NationalIdNumber = $"NID-{h.Email.GetHashCode()}"
                });
            }
            await dbContext.SaveChangesAsync();

            // ── Seed 3 Workers ──
            var workers = new[]
            {
            new { Email = "worker1@maidsandnannies.local", Name = "فاطمة حسن", Phone = "01220000001", NationalityId = 65, MonthlyRate = 5000m, DailyRate = 200m, HourlyRate = 30m, CurrencyId = 1 },
            new { Email = "worker2@maidsandnannies.local", Name = "عائشة محمود", Phone = "01220000002", NationalityId = 65, MonthlyRate = 200m, DailyRate = 0m,    HourlyRate = 0m,  CurrencyId = 2 },
            new { Email = "worker3@maidsandnannies.local", Name = "خديجة علي",   Phone = "01220000003", NationalityId = 65, MonthlyRate = 800m, DailyRate = 0m,    HourlyRate = 0m,  CurrencyId = 3 },
        };

            foreach (var w in workers)
            {
                if (await userManager.FindByEmailAsync(w.Email) is not null) continue;

                var user = new ApplicationUser
                {
                    UserName = w.Email,
                    Email = w.Email,
                    FullName = w.Name,
                    PreferredLanguage = "ar",
                    Role = UserRole.Worker,
                    EmailConfirmed = true,
                    PhoneNumber = w.Phone
                };

                var result = await userManager.CreateAsync(user, "Worker@12345");
                if (!result.Succeeded) continue;

                await userManager.AddToRoleAsync(user, UserRole.Worker.ToString());

                dbContext.WorkerProfiles.Add(new WorkerProfile
                {
                    UserId = user.Id,
                    NationalityId = w.NationalityId,
                    CountryId = null,
                    StateId = null,
                    Bio = string.Empty,
                    ExperienceYears = 5,
                    MonthlyRate = w.MonthlyRate,
                    DailyRate = w.DailyRate,
                    HourlyRate = w.HourlyRate,
                    VerificationStatus = VerificationStatus.Pending,
                    CurrencyId = w.CurrencyId
                });
            }
            await dbContext.SaveChangesAsync();
        }

        public static async Task SeedAOnProductionsync(UserManager<ApplicationUser> userManager, IApplicationDbContext dbContext , RoleManager<IdentityRole> roleManager  , string adminEmail  , string adminPassword ,
            string homeownerEmail  ,string homewnerPassword ,
            string workerEmail , string workerPassword)
        {

            foreach (var role in Enum.GetNames<UserRole>())
                if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));

            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var adminUser = new MaidsAndNannies.Domain.Entities.Identity.ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Platform Admin",
                    PreferredLanguage = "ar",
                    Role = UserRole.Admin,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
            }


            if (await userManager.FindByEmailAsync(homeownerEmail) is null)
            {
                var homeownerUser = new MaidsAndNannies.Domain.Entities.Identity.ApplicationUser
                {
                    UserName = homeownerEmail,
                    Email = homeownerEmail,
                    FullName = "Platform Homeowner",
                    PreferredLanguage = "ar",
                    Role = UserRole.Homeowner,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(homeownerUser, homewnerPassword);
                if (createResult.Succeeded)
                    await userManager.AddToRoleAsync(homeownerUser, UserRole.Homeowner.ToString());



                dbContext.HomeownerProfiles.Add(new HomeownerProfile
                {
                    UserId = homeownerUser.Id,
                    Address = $"عنوان",
                    City = "القاهرة",
                    State = "مصر",
                    WhatsAppNumber = "1234567",
                    VerificationStatus = VerificationStatus.Pending,
                    NationalIdNumber = null
                });

                await dbContext.SaveChangesAsync();
            }


            if (await userManager.FindByEmailAsync(workerEmail) is null)
            {
                var workerUser = new MaidsAndNannies.Domain.Entities.Identity.ApplicationUser
                {
                    UserName = workerEmail,
                    Email = workerEmail,
                    FullName = "Platform Worker",
                    PreferredLanguage = "ar",
                    Role = UserRole.Worker,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(workerUser, workerPassword);
                if (createResult.Succeeded)
                    await userManager.AddToRoleAsync(workerUser, UserRole.Worker.ToString());

                dbContext.WorkerProfiles.Add(new WorkerProfile
                {
                    UserId = workerUser.Id,
                    NationalityId = 65,
                    CountryId = null,
                    StateId = null,
                    Bio = string.Empty,
                    ExperienceYears = 5,
                    MonthlyRate = 5000,
                    DailyRate = 500,
                    HourlyRate = 88,
                    VerificationStatus = VerificationStatus.Pending,
                    CurrencyId = (await dbContext.Currencies.FirstOrDefaultAsync())?.Id??0,
                    IsAvailable = false
                });

                await dbContext.SaveChangesAsync();
            }



        }
    }

}

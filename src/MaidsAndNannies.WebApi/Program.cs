using Azure.Core;
using FluentValidation;
using MaidsAndNannies.Application;
using MaidsAndNannies.Application.Common.Behaviors;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Contracts;
using MaidsAndNannies.Application.Features.Homeowner.Commands.UpdateProfile;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MaidsAndNannies.Infrastructure;
using MaidsAndNannies.Infrastructure.Persistence;
using MaidsAndNannies.WebApi.Middleware;
using MaidsAndNannies.WebApi.Services;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GeoSeeder>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    });
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"])
        .AllowAnyHeader()
        .AllowAnyMethod()));



builder.Services.AddHttpClient();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var geoSeeder = scope.ServiceProvider.GetRequiredService<GeoSeeder>();
    await geoSeeder.SeedAsync();

    if (app.Environment.IsDevelopment())
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    foreach (var role in Enum.GetNames<UserRole>())
        if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));

    var adminEmail = builder.Configuration["AdminSeed:Email"] ?? "admin@maidsandnannies.local";
    var adminPassword = builder.Configuration["AdminSeed:Password"] ?? "Admin@12345";
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MaidsAndNannies.Domain.Entities.Identity.ApplicationUser>>();

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

    if (app.Environment.IsDevelopment())
    {
        if (!await dbContext.Currencies.AnyAsync())
        {
            await dbContext.Currencies.AddRangeAsync(new List<Currency>
            {
                new Currency { Id = 1, Code = "EGP", Symbol = "E£", NameAr = "جنيه مصري", NameEn = "Egyptian Pound", RateToEgp = 1m, IsActive = true },
                new Currency { Id = 2, Code = "USD", Symbol = "$", NameAr = "دولار أمريكي", NameEn = "US Dollar", RateToEgp = 48.5m, IsActive = true },
                new Currency { Id = 3, Code = "SAR", Symbol = "﷼", NameAr = "ريال سعودي", NameEn = "Saudi Riyal", RateToEgp = 12.9m, IsActive = true }
            });
            await dbContext.SaveChangesAsync();
        }

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
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
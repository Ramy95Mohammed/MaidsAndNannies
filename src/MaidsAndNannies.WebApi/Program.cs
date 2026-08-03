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
using MaidsAndNannies.WebApi.Localization;
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

builder.Services.AddHostedService<BookingReminderService>();

var app = builder.Build();

MessageLocalizer.Initialize(app.Environment.WebRootPath);

using (var scope = app.Services.CreateScope())
{
    var geoSeeder = scope.ServiceProvider.GetRequiredService<GeoSeeder>();
    await geoSeeder.SeedAsync();

    if (app.Environment.IsDevelopment())
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


    if (!await dbContext.Policies.AnyAsync())
    {
        dbContext.Policies.AddRange(
            new Policy
            {
                Key = "terms",
                SortOrder = 1,
                IsActive = true,
                TitleAr = "شروط الاستخدام",
                TitleEn = "Terms of Use",
                ContentAr = "منصة مادام والخادمات والجليسات هي منصة وسيطة تربط بين أصحاب المنازل والعاملات. باستخدامك للمنصة فأنت توافق على هذه الشروط.\nيجب أن تكون جميع البيانات المقدمة صحيحة ودقيقة، ويلتزم الطرفان باحترام المواعيد والعقود المتفق عليها.\nتلتزم العاملات بتأدية العمل المتفق عليه بأمانة واحترافية، ويلتزم أصحاب المنازل بدفع المبالغ المتفق عليها عبر وسائل الدفع المعتمدة على المنصة.\nتحتفظ المنصة بحقها في إيقاف أي حساب يخالف هذه الشروط أو يستخدم المنصة بشكل غير قانوني.",
                ContentEn = "Madams, Maids and Nannies is an intermediary platform connecting homeowners and workers. By using the platform you agree to these terms.\nAll provided information must be accurate, and both parties must respect the agreed schedules and contracts.\nWorkers commit to performing the agreed work honestly and professionally, and homeowners commit to paying agreed amounts through the platform's approved payment methods.\nThe platform reserves the right to suspend any account that violates these terms or uses the platform illegally."
            },
            new Policy
            {
                Key = "privacy",
                SortOrder = 2,
                IsActive = true,
                TitleAr = "سياسة الخصوصية",
                TitleEn = "Privacy Policy",
                ContentAr = "نحترم خصوصيتك ونلتزم بحماية بياناتك الشخصية.\nتُستخدم بياناتك فقط لأغراض التشغيل: التحقق من الهوية، التواصل بين الطرفين، وإتمام الحجوزات والمدفوعات.\nلن نشارك بياناتك مع أي طرف ثالث إلا عند الضرورة القانونية أو لتشغيل الخدمة نفسها مثل بوابات الدفع.\nيمكنك طلب حذف حسابك وبياناتك في أي وقت من خلال التواصل مع الدعم الفني.",
                ContentEn = "We respect your privacy and are committed to protecting your personal data.\nYour data is used only for operational purposes: identity verification, communication between parties, and completing bookings and payments.\nWe never share your data with third parties except when legally required or needed to operate the service itself, such as payment gateways.\nYou can request deletion of your account and data at any time by contacting support."
            },
            new Policy
            {
                Key = "disclaimer",
                SortOrder = 3,
                IsActive = true,
                TitleAr = "إخلاء المسؤولية",
                TitleEn = "Disclaimer",
                ContentAr = "المنصة مجرد وسيط بين أصحاب المنازل والعاملات، ولا تعمل كصاحب عمل لأي منهما، ولا تضمن سلوك أو كفاءة أي عاملة.\nالمنصة غير مسؤولة عن أي سرقة أو تلف أو أضرار أو خلافات تحدث بين الطرفين أثناء العمل أو خارجه.\nننصح بفحص العاملات وتوثيق الممتلكات الثمينة والإبلاغ الفوري عن أي مشكلة.\nجميع التعاملات المالية الجارية خارج المنصة تتم على مسؤولية الطرفين فقط.",
                ContentEn = "The platform is only an intermediary between homeowners and workers. It is not an employer of either party and does not guarantee the conduct or competence of any worker.\nThe platform is not responsible for any theft, damage, or disputes occurring between the parties during or outside work.\nWe recommend inspecting workers, securing valuables, and reporting any problem immediately.\nAny financial transactions taking place outside the platform are the sole responsibility of both parties."
            },
            new Policy
            {
                Key = "commission",
                SortOrder = 4,
                IsActive = true,
                TitleAr = "عمولة المنصة وسياسة الاستبدال",
                TitleEn = "Commission & Replacement Policy",
                ContentAr = "تُحسب عمولة المنصة كنسبة مئوية من إجمالي قيمة الحجز حسب نوع الخدمة، كما هو موضح في صفحة الحجز.\nعند طلب استبدال العاملات دون سبب مشروع بشكل متكرر، يجوز للمنصة رفض الطلب أو تقييده.\nالاستبدال بسبب خطأ من العاملات يتم دون رسوم إضافية.\nتُلغى العمولة عند إلغاء الحجز قبل بدء العمل وفق سياسة الدفع.",
                ContentEn = "The platform commission is calculated as a percentage of the total booking value according to the service type, as shown on the booking page.\nIf replacement requests are repeated without legitimate reason, the platform may refuse or restrict them.\nReplacement due to a worker's fault is made at no additional cost.\nThe commission is refunded when a booking is cancelled before work starts, per the payment policy."
            },
            new Policy
            {
                Key = "payment",
                SortOrder = 5,
                IsActive = true,
                TitleAr = "سياسة الدفع والاسترداد",
                TitleEn = "Payment & Refund Policy",
                ContentAr = "يتم الدفع عبر وسائل الدفع المعتمدة على المنصة، ولا يُطلب منك أبدًا الدفع خارجها.\nإذا ألغيت الحجز قبل بدء العمل تُسترجع كامل المبالغ المدفوعة خلال 3 إلى 7 أيام عمل.\nإذا بدأ العمل يحق للمنصة الاحتفاظ بنسبة العمل المنفذ من الرسوم.\nفي حالة وجود نزاع يمكنك فتح تذكرة مع الدعم الفني خلال 7 أيام من تاريخ الحجز.",
                ContentEn = "Payments are made through the platform's approved methods; you will never be asked to pay outside the platform.\nIf you cancel before work starts, full paid amounts are refunded within 3 to 7 business days.\nIf work has started, the platform may retain the portion of fees corresponding to work performed.\nIn case of a dispute, you can open a ticket with support within 7 days of the booking date."
            });
        await dbContext.SaveChangesAsync();
    }


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

app.UseMiddleware<ResponseLocalizationMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
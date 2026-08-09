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
        ContentAr = "رفيقة (Rafeeqa) هي منصة وسيطة تربط بين أصحاب المنازل والعاملات. باستخدامك للمنصة فأنت توافق على هذه الشروط.\r\nيجب أن تكون جميع البيانات المقدمة صحيحة ودقيقة، وتخضع الحسابات لمراجعة واعتماد الإدارة قبل تفعيلها.\r\nتتمتع الحجوزات يومي أو شهري أو بالساعة، وتمر الحجوزات بحالات متتالية: بانتظار تأكيد العاملة،انتظار سداد العمولة، تم السداد، الحجز النشط، ثم المكتمل أو الملغى.\r\nالاستبدال متاح وفق سياسة الاستبدال المعتمدة، ويجوز للمنصة رفض الطلبات المتكررة غير المبررة.\r\nتحتفظ المنصة بحقها في إيقاف أي حساب يخالف هذه الشروط أو يستخدم المنصة لأي غرض غير قانوني.",
        ContentEn = "Rafeeqa is an intermediary platform connecting homeowners with domestic workers. By using the platform, you agree to these terms and conditions.\r\n\r\nAll information provided must be true and accurate, and accounts are subject to review and approval by the administration before activation.\r\n\r\nBookings are available daily, monthly, or hourly, and progress through the following stages: awaiting worker confirmation, awaiting commission payment, payment completed, active booking, and then completed or canceled.\r\n\r\nReplacements are available according to the approved replacement policy, and the platform may reject unjustified duplicate requests.\r\n\r\nThe platform reserves the right to suspend any account that violates these terms and conditions or uses the platform for any illegal purpose."
    },
    new Policy
    {
        Key = "privacy",
        SortOrder = 2,
        IsActive = true,
        TitleAr = "سياسة الخصوصية",
        TitleEn = "Privacy Policy",
        ContentAr = "نحترم خصوصيتك ونلتزم بحماية بياناتك الشخصية.\r\nتشمل البيانات التي نجمعها: الاسم، البريد الإلكتروني، رقم الهاتف والواتساب، \r\nتُستخدم بياناتك فقط لتشغيل المنصة: اعتماد الحسابات، إتمام الحجوزات والمدفوعات، إرسال الإشعارات، وتمكين التواصل بين الطرفين بعد بدء الحجز.\r\nلا نبيع بياناتك لأي جهة، ولا نشاركها مع أطراف ثالثة .\r\nيمكنك طلب تعديل بياناتك أو حذف حسابك في أي وقت بالتواصل مع الدعم الفني.",
        ContentEn = "We respect your privacy and are committed to protecting your personal data.\r\n\r\nThe data we collect includes: your name, email address, phone number, and WhatsApp number.\r\n\r\nYour data is used solely to operate the platform: account verification, processing bookings and payments, sending notifications, and enabling communication between the parties after a booking is initiated.\r\n\r\nWe do not sell your data to any third party, nor do we share it with any third parties.\r\n\r\nYou can request to modify your data or delete your account at any time by contacting technical support."
    },
    new Policy
    {
        Key = "disclaimer",
        SortOrder = 3,
        IsActive = true,
        TitleAr = "إخلاء المسؤولية",
        TitleEn = "Disclaimer",
        ContentAr = "رفيقية مجرد وسيط بين أصحاب المنازل والعاملات، ولا تعمل كصاحب عمل لأي منهما، ولا تضمن سلوك أو كفاءة أو أمانة أي عاملة أو صاحب منزل.\r\nلا تتحمل المنصة مسؤولية أي سرقة أو تلف أو أضرار أو خلافات تحدث بين الطرفين أثناء العمل أو خارجه.\r\nننصح بفحص العاملات وحفظ الممتلكات الثمينة  والتقييم بطريقة عادلة وشفافة عبر المنصة.\r\nأي تعاملات مالية تتم خارج وسائل الدفع المعتمدة على المنصة تتم على مسؤولية الطرفين فقط."
    ,
        ContentEn = "Rafiqia is merely an intermediary between homeowners and domestic workers. It does not act as an employer for either party and does not guarantee the conduct, competence, or honesty of any worker or homeowner.\r\n\r\nThe platform is not responsible for any theft, damage, losses, or disputes that may occur between the parties during or outside of work.\r\n\r\nWe advise you to thoroughly inspect domestic workers, safeguard valuables, and conduct fair and transparent evaluations through the platform.\r\n\r\nAny financial transactions conducted outside of the platform's approved payment methods are the sole responsibility of the parties involved."
    },
    new Policy
    {
        Key = "commission",
        SortOrder = 4,
        IsActive = true,
        TitleAr = "عمولة المنصة وسياسة الاستبدال",
        TitleEn = "Commission & Replacement Policy",
        ContentAr = "تُحسب عمولة المنصة وفق الإعدادات المعتمدة: إما عمولة المنصة فقط، أو العمولة مضافًا إليها أول مرتب للعاملة محولًا إلى الجنيه المصري، كما هو موضح في صفحة الحجز.\nتسدد العمولة عبر وسائل الدفع المعتمدة (فودافون كاش، إنستاباي) قبل بدء الحجز، مع رفع إثبات الدفع الذي تراجعه الإدارة قبل تفعيل الحجز.\nخدمات في الاشتراك الشهري المتجدد أو عمولة واحدة حسب نوع الخدمة المختار.\nالاستبدال: يحق لصاحب المنزل طلب الاستبدال مرات محددة حسب نوعه، وب° الحساب؛ الاستبدال بسبب خطأ من العاملة يأتي دون رسوم إضافية مع مراعاة الحد الأقصى، والاستبدال برغبة شخصية محدود العدد ويتم تأكيد العاملة الجديدة قبل التنفيذ.\nعند الاستبدال يُغلق الحجز القديم ويُفتح حجز مستقل جديد مكانه.\nيجوز للمنصة رفض أو تقييد الاستبدال في حال تجاوز الحدود المقررة.",
        ContentEn = "The platform commission is calculated based on configured mode: commission only, or commission plus the worker's first salary converted to EGP, as shown on the booking page.\nThe commission is paid through the approved payment methods (Vodafone Cash, InstaPay) before the booking starts, with proof upload reviewed by admins before activation.\nServices are either one-time commission or a monthly renewable subscription per option.\nReplacement: bookings may be replaced a limited number of times according to the account type; replacement due to worker fault costs no additional fee within the max limit, while personal-preference replacement is limited and confirmed with the new worker before execution.\nWhen a replacement happens, the old booking is closed and an independent new booking is opened instead.\nThe platform may refuse or restrict replacement when limits are exceeded."
    },
    new Policy
    {
        Key = "payment",
        SortOrder = 5,
        IsActive = true,
        TitleAr = "سياسة الدفع والاسترداد",
        TitleEn = "Payment & Refund Policy",
        ContentAr = "الدفع يتم حصريًا عبر وسائل الدفع المعتمدة على المنصة (فودافون كاش أو إنستاباي)، ولا تكون مطالبًا أبدًا بالدفع خارجها.\nبعد السداد يرجى رفع إثبات الدفع (صورة التحويل مع رقم العملية) في صفحة الحجز، وتقوم الإدارة بمراجعة الإثبات إجاب قبول السداد وبدء الحجز.\nإذا ألغيت الحجز قبل بدء العمل ولم يُقبل السداد، تُلغى العمولة ولا يُخصم منك شيء.\nإذا بدأ الحجز أو تم سداد العمولة، تنطبق سياسة الاسترداد على الحالات المحددة فقط، ويُحتفظ بنسبة الرسوم المقابلة للعمل المنفذ.\nفي حال وجود نزاع يمكنك التواصل مع الإدارة خلال 7 أيام من تاريخ الحجز."
,
        ContentEn = "Payments are made exclusively through the platform's approved methods (Vodafone Pay or InstaPay); you will never be asked to pay elsewhere.\nAfter paying, upload your payment proof (transfer photo with transaction reference) in the booking page; admins review the proof and activate the booking.\nIf you cancel before the booking starts and payment was not accepted, the charge is voided and nothing is deducted.\nIf the work has started or payment was accepted, refunds are considered only in the defined cases, and the platform retains fees corresponding to work performed.\nIn case of any dispute, contact the admin within 7 days of the booking date."
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
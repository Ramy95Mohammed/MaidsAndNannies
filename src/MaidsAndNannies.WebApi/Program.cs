using MaidsAndNannies.Application;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MaidsAndNannies.Infrastructure;
using MaidsAndNannies.Infrastructure.Persistence;
using MaidsAndNannies.Infrastructure.Seed;
using MaidsAndNannies.WebApi.Localization;
using MaidsAndNannies.WebApi.Middleware;
using MaidsAndNannies.WebApi.Services;
using MaidsPlatform.API.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
    policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200" , "http://localhost:4201"
    ,"https://rafeeqa.prime-devv.online"])
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

    

    await PoliciesSeed.SeedAsync(dbContext);

    await CurrenciesSeed.SeedAsync(dbContext);

    await AppSettingSeed.SeedAsync(dbContext);

    var adminEmail = builder.Configuration["AdminSeed:Email"] ?? "admin@maidsandnannies.local";
    var adminPassword = builder.Configuration["AdminSeed:Password"] ?? "Admin@12345";

    var homeownerEmail = builder.Configuration["HomeownerSeed:Email"] ?? "homeowner@maidsandnannies.local";
    var homewnerPassword = builder.Configuration["HomeownerSeed:Password"] ?? "Homeowner@12345";

    var workerEmail = builder.Configuration["WorkerSeed:Email"] ?? "worker@maidsandnannies.local";
    var workerPassword = builder.Configuration["WorkerSeed:Password"] ?? "Worker@12345";

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MaidsAndNannies.Domain.Entities.Identity.ApplicationUser>>();
    
    
    await UsersSeed.SeedAOnProductionsync(userManager, dbContext, roleManager, adminEmail, adminPassword,
        homeownerEmail , homewnerPassword , workerEmail , workerPassword);


    if (app.Environment.IsDevelopment())
    {       
        //await UsersSeed.SeedAOnDevelopsync(userManager, dbContext);
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
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
using Azure.Core;
using FluentValidation;
using MaidsAndNannies.Application;
using MaidsAndNannies.Application.Common.Behaviors;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Contracts;
using MaidsAndNannies.Application.Features.Homeowner.Commands.UpdateProfile;
using MaidsAndNannies.Domain.Entities;
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
        var homeownerEmail = "homeowner@maidsandnannies.local";
        if (await userManager.FindByEmailAsync(homeownerEmail) is null)
        {

            var homeownerUser = new MaidsAndNannies.Domain.Entities.Identity.ApplicationUser
            {
                UserName = homeownerEmail,
                Email = homeownerEmail,
                FullName = "Platform Homeowner",
                PreferredLanguage = "ar",
                Role = UserRole.Homeowner,
                EmailConfirmed = true,
                PhoneNumber = "11245454878787"
            };

            var homeownerCreateResult = await userManager.CreateAsync(homeownerUser, "Homeowner@12345");
            if (homeownerCreateResult.Succeeded)
                await userManager.AddToRoleAsync(homeownerUser, UserRole.Homeowner.ToString());


            var homeownerProfile = new HomeownerProfile
            {
                UserId = homeownerUser.Id,
                Address = "my address",
                City = "my city",
                State = "my state",
                WhatsAppNumber = "454564578745455",
                VerificationStatus = VerificationStatus.Pending,
                NationalIdNumber = "id number"
            };

            dbContext.HomeownerProfiles.Add(homeownerProfile);
            await dbContext.SaveChangesAsync();

        }


        var workerEmail = "worker@maidsandnannies.local";
        if (await userManager.FindByEmailAsync(workerEmail) is null)
        {

            var workerUser = new MaidsAndNannies.Domain.Entities.Identity.ApplicationUser
            {
                UserName = workerEmail,
                Email = workerEmail,
                FullName = "Platform Worker",
                PreferredLanguage = "ar",
                Role = UserRole.Worker,
                EmailConfirmed = true,
                PhoneNumber = "01254545454454"
            };

            var workerCreateResult = await userManager.CreateAsync(workerUser, "Worker@12345");
            if (workerCreateResult.Succeeded)
                await userManager.AddToRoleAsync(workerUser, UserRole.Worker.ToString());

            var workerProfile = new WorkerProfile
            {
                UserId = workerUser.Id,
                NationalityId = 65,
                CountryId = null,
                StateId = null,
                Bio =  string.Empty,
                ExperienceYears = 5,
                MonthlyRate = 5000,
                VerificationStatus = VerificationStatus.Pending
            };            

            dbContext.WorkerProfiles.Add(workerProfile);
            await dbContext.SaveChangesAsync();
        }
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
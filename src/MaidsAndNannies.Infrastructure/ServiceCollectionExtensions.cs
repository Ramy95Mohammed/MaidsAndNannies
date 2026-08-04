using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Contracts;
using MaidsAndNannies.Application.Features.Booking.Common;
using MaidsAndNannies.Application.Services;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Infrastructure.Persistence;
using MaidsAndNannies.Infrastructure.Services;
using MaidsAndNannies.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace MaidsAndNannies.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IFileStorage, LocalPrivateFileStorage>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

  
        services.AddScoped<INotificationService, NotificationService>();        
        services.AddScoped<ICalculateBookingCommissionData, CalculateBookingCommissionData>();        

        return services;
    }
}

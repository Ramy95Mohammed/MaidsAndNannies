using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Admin.Common;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAdminDashboard;

public sealed class GetAdminDashboardQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken ct)
    {
        var totalUsers = await dbContext.Users.CountAsync(ct);
        var totalHomeowners = await dbContext.HomeownerProfiles.CountAsync(ct);
        var totalWorkers = await dbContext.WorkerProfiles.CountAsync(ct);
        var totalBookings = await dbContext.Bookings.CountAsync(ct);
        var activeBookings = await dbContext.Bookings.CountAsync(b => b.Status == BookingStatus.Active, ct);
        var pendingVerifications =
            await dbContext.HomeownerProfiles.CountAsync(h => h.VerificationStatus == VerificationStatus.Pending, ct)
            + await dbContext.WorkerProfiles.CountAsync(w => w.VerificationStatus == VerificationStatus.Pending, ct);
        var pendingPayments = await dbContext.PaymentProofs.CountAsync(p => !p.IsConfirmed, ct);

        return new AdminDashboardDto(
            totalUsers, totalHomeowners, totalWorkers, totalBookings,
            activeBookings, pendingVerifications, pendingPayments);
    }
}
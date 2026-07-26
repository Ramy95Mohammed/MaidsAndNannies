using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.AcceptApplication;

public sealed class AcceptApplicationCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AcceptApplicationCommand, int>
{
    public async Task<int> Handle(AcceptApplicationCommand r, CancellationToken ct)
    {
        var post = await dbContext.JobPosts
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == r.PostId && j.HomeownerId == r.HomeownerId, ct)
            ?? throw new KeyNotFoundException("الإعلان غير موجود");

        var app = post.Applications.FirstOrDefault(a => a.Id == r.AppId)
            ?? throw new KeyNotFoundException("الطلب غير موجود");
        if (app.Status != ApplicationStatus.Pending)
            throw new InvalidOperationException("الطلب لم يعد قيد الانتظار");

        app.Status = ApplicationStatus.Accepted;

        var booking = new Booking
        {
            HomeownerId = r.HomeownerId,
            WorkerId = app.WorkerId,
            ServiceType = post.Specialization,
            BookingType = post.BookingType,
            Quantity = post.Quantity,
            StartDate = post.StartDate,
            MonthlySalary = post.MonthlySalary,
            DailySalary = post.DailySalary,
            HourlySalary = post.HourlySalary,
            CommissionType = post.CommissionType,
            Status = BookingStatus.Pending,
            JobPostId = r.PostId,
            ReplacementCount = 0
        };
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync(ct);
        return booking.Id;
    }
}
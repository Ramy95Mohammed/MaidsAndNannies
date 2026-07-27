using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Contracts;
using MaidsAndNannies.Domain.Entities;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.UploadPaymentProof;

public sealed class UploadPaymentProofCommandHandler(
    IApplicationDbContext dbContext,
    IFileStorage fileStorage)
    : IRequestHandler<UploadPaymentProofCommand, Unit>
{
    public async Task<Unit> Handle(UploadPaymentProofCommand request, CancellationToken ct)
    {
        var booking = await dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.HomeownerId == request.HomeownerId, ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.Status != BookingStatus.WaitingPayment)
            throw new InvalidOperationException("الحجز ليس بانتظار الدفع");

        string? proofUrl = null;
        if (request.ProofImageContent is not null && request.ProofImageFileName is not null)
            proofUrl = await fileStorage.SavePublicAsync(
                request.ProofImageContent, request.ProofImageFileName,
                $"payments/{request.BookingId}", ct);

        dbContext.PaymentProofs.Add(new PaymentProof
        {
            BookingId = request.BookingId,
            HomeownerId = request.HomeownerId,
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount,
            CommissionAmount = request.CommissionAmount,
            ProofImageUrl = proofUrl ?? string.Empty,
            TransactionReference = request.TransactionReference
        });

        booking.Status = BookingStatus.PaymentSubmitted;
        booking.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
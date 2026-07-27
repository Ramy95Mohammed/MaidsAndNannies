using MaidsPlatform.API.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.UploadPaymentProof;

public sealed record UploadPaymentProofCommand(
    int BookingId,
    string HomeownerId,
    PaymentMethod PaymentMethod,
    decimal Amount,
    decimal CommissionAmount,
    string? TransactionReference,
    Stream? ProofImageContent,
    string? ProofImageFileName) : IRequest<Unit>;
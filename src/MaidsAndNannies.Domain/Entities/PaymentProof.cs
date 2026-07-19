using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Domain.Entities;

public class PaymentProof : Entity
{    
    public int BookingId { get; set; }
    public string HomeownerId { get; set; } = string.Empty;

    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string ProofImageUrl { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }

    public bool IsConfirmed { get; set; } = false;
    public string? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Booking Booking { get; set; } = null!;
    public virtual ApplicationUser Homeowner { get; set; } = null!;
}

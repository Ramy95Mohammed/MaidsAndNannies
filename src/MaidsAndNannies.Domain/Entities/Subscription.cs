using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Domain.Entities;

public class Subscription : Entity
{    
    public string HomeownerId { get; set; } = string.Empty;

    public CommissionType PlanType { get; set; }
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentProofImageUrl { get; set; }
    public string? PaymentConfirmedBy { get; set; }
    public DateTime? PaymentConfirmedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? TransactionReference { get; set; }

    public virtual ApplicationUser Homeowner { get; set; } = null!;
}

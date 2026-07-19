using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Domain.Entities;
public class Booking : Entity
{    
    public string HomeownerId { get; set; } = string.Empty;
    public string WorkerId { get; set; } = string.Empty;

    public Specialization ServiceType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MonthlySalary { get; set; }

    public decimal CommissionAmount { get; set; }
    public CommissionType CommissionType { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentProofImageUrl { get; set; }
    public string? PaymentConfirmedBy { get; set; }
    public DateTime? PaymentConfirmedAt { get; set; }

    public bool IsPaid { get; set; } = false;
    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ApplicationUser Homeowner { get; set; } = null!;
    public virtual ApplicationUser Worker { get; set; } = null!;
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

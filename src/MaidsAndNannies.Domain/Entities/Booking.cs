using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Domain.Entities;
public class Booking : Entity
{    
    public string HomeownerId { get; set; } = string.Empty;
    public string WorkerId { get; set; } = string.Empty;

    public Specialization ServiceType { get; set; }
    public BookingType BookingType { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MonthlySalary { get; set; }
    public decimal DailySalary { get; set; }
    public decimal HourlySalary { get; set; }
    public decimal TotalAmount { get; set; }         // الجديد
    public decimal CommissionAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
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

    public int ReplacementCount { get; set; }
    public int? OriginalWorkerId { get; set; }
    public string? AdminNotes { get; set; }

    /// <summary>سبب آخر عملية استبدال تمت على هذا الحجز (للتتبع/العرض لصاحبة المنزل).</summary>
    public ReplacementReason? LastReplacementReason { get; set; }

    /// <summary>
    /// للحجوزات اليومية/الساعية فقط: يشير إلى الحجز الأصلي الذي نتج عنه هذا الحجز
    /// عند إنشاء حجز جديد مستقل بدل تعديل الحجز القديم.
    /// </summary>
    public int? ReplacedFromBookingId { get; set; }

    public int? JobPostId { get; set; }
    public int? CurrencyId { get; set; }
    public virtual Currency? Currency { get; set; }

    public virtual ApplicationUser Homeowner { get; set; } = null!;
    public virtual ApplicationUser Worker { get; set; } = null!;
    public virtual WorkerProfile OriginalWorker { get; set; } = null!;
    public virtual JobPost? JobPost { get; set; }
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();    

}

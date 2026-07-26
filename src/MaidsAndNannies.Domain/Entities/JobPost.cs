using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Domain.Entities;

public class JobPost : Entity
{
    public string HomeownerId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? SanitizedDescription { get; set; }
    public decimal MonthlySalary { get; set; }
    public decimal DailySalary { get; set; }
    public decimal HourlySalary { get; set; }
    public Specialization Specialization { get; set; }
    public BookingType BookingType { get; set; }
    public CommissionType CommissionType { get; set; }
    public DateTime StartDate { get; set; }
    public int Quantity { get; set; } = 1;
    public int CurrencyId { get; set; } = 1; // default EGP
    public JobPostStatus PostStatus { get; set; } = JobPostStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ApplicationUser Homeowner { get; set; } = null!;
    public virtual Currency Currency { get; set; } = null!;
    public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
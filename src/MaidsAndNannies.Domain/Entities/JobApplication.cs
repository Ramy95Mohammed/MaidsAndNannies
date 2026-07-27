using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;

namespace MaidsAndNannies.Domain.Entities;

public enum ApplicationStatus { Pending = 0, Accepted = 1, Rejected = 2 }

public class JobApplication : Entity
{
    public int JobPostId { get; set; }
    public string WorkerId { get; set; } = string.Empty;
    public string? Message { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual JobPost JobPost { get; set; } = null!;
    public virtual ApplicationUser Worker { get; set; } = null!;
}
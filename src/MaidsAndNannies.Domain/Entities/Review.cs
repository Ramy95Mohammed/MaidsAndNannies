using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;
namespace MaidsAndNannies.Domain.Entities;

public class Review : Entity
{    
    public int BookingId { get; set; }
    public string ReviewerId { get; set; } = string.Empty;
    public string RevieweeId { get; set; } = string.Empty;

    public int Rating { get; set; }
    public string? Comment { get; set; }

    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Booking Booking { get; set; } = null!;
    public virtual ApplicationUser Reviewer { get; set; } = null!;
    public virtual ApplicationUser Reviewee { get; set; } = null!;
}

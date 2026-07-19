using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;


namespace MaidsAndNannies.Domain.Entities;

public class Message : Entity
{    
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public int? BookingId { get; set; }

    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ApplicationUser Sender { get; set; } = null!;
    public virtual ApplicationUser Receiver { get; set; } = null!;
    public virtual Booking? Booking { get; set; }
}

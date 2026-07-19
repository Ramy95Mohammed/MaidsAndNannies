using MaidsAndNannies.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace MaidsAndNannies.Domain.Entities.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string PreferredLanguage { get; set; } = "ar";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public  HomeownerProfile? HomeownerProfile { get; set; }
    public  WorkerProfile? WorkerProfile { get; set; }
    public  ICollection<Booking> BookingsAsHomeowner { get; set; } = new List<Booking>();
    public  ICollection<Booking> BookingsAsWorker { get; set; } = new List<Booking>();
    public  ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public  ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public  ICollection<Review> ReviewsWritten { get; set; } = new List<Review>();
    public  ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
}

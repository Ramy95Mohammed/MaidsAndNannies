using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;


namespace MaidsAndNannies.Domain.Entities;

public class Notification : Entity
{    
    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public string? Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ApplicationUser User { get; set; } = null!;
}

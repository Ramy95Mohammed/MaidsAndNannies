using MaidsAndNannies.Domain.Common;

namespace MaidsAndNannies.Domain.Entities;

public sealed class PasswordResetRequest : Entity
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public PasswordResetStatus Status { get; set; } = PasswordResetStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public enum PasswordResetStatus
{
    Pending = 0,
    Sent = 1,
    Used = 2,
    Expired = 3
}
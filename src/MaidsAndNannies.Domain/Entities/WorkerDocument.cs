using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Domain.Entities;
public class WorkerDocument : Entity
{    
    public int WorkerId { get; set; }
    public  WorkerProfile? Worker { get; set; }
    public DocumentType Type { get; set; }
    public string DocumentImageUrl { get; set; } = string.Empty;
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}

using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Domain.Entities;
public class HomeownerProfile : Entity
{    
    public string UserId { get; set; } = string.Empty;

    public string NationalIdNumber { get; set; } = string.Empty;
    public string NationalIdImage { get; set; } = string.Empty;
    public string SelfieImage { get; set; } = string.Empty;
    public string? ProofOfAddressImage { get; set; }

    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? District { get; set; }

    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    public string? VerificationNotes { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }

    public CommissionType CommissionType { get; set; } = CommissionType.OneTime;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
}

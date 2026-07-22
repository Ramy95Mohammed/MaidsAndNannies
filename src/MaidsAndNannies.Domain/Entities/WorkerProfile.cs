using MaidsAndNannies.Domain.Common;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Domain.Entities;

public class WorkerProfile : Entity
{
    public string UserId { get; set; } = string.Empty;//

    // Personal Info
    public int NationalityId { get; set; }//
    public string NationalIdNumber { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;

    // Passport (optional for foreign workers)
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiryDate { get; set; }
    public string? PassportCountry { get; set; }

    // Professional Info
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; } = 0;
    public string? PreviousEmployer { get; set; }
    public string? Languages { get; set; }

    // Specialization
    public ICollection<WorkerSpecializationSpec>? WorkerSpecializationSpecs { get; set; } = new List<WorkerSpecializationSpec>();
    public bool IsLiveIn { get; set; } = false;

    // Rates
    public decimal? HourlyRate { get; set; }
    public decimal? MonthlyRate { get; set; }
    public int Currency { get; set; }

    // Location
    public int? StateId { get; set; }
    public int? CountryId { get; set; }
    public int? CityId { get; set; }
    public string? Address { get; set; }

    // Rating
    public decimal AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;

    // Status
    public bool IsAvailable { get; set; } = true;
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    public string? VerificationNotes { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<WorkerDocument> Documents { get; set; } = new List<WorkerDocument>();
}


public sealed class WorkerSpecializationSpec : Entity
{
    public int WorkerProfileId { get; set; }
    public WorkerProfile WorkerProfile { get; set; } = null!;
    public Specialization WorkerSpecialization { get; set; }
}
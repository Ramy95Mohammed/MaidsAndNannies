using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsAndNannies.Infrastructure.Persistence;
using MaidsPlatform.API.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace MaidsAndNannies.WebApi.Controllers
{
    [Authorize]
    public class WorkerController : BaseApiController
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxDocumentSizeBytes = 5 * 1024 * 1024; // 5 MB, matches the frontend's maxFileSize

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public WorkerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetWorkers(
            [FromQuery] int? cityId,
            [FromQuery] Specialization? specialization,
            [FromQuery] bool? isLiveIn,
            [FromQuery] decimal? maxRate,
            [FromQuery] int? nationalityId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.WorkerProfiles
                .Include(w => w.User)
                .Where(w => w.IsAvailable && w.VerificationStatus == VerificationStatus.Approved)
                .AsQueryable();

            if (cityId.HasValue)
                query = query.Where(w => w.CityId == cityId);

            if (specialization.HasValue)
                query = query.Where(w => w.WorkerSpecializationSpecs != null &&
                w.WorkerSpecializationSpecs.Select(s => s.WorkerSpecialization).Contains(specialization.Value));

            if (isLiveIn.HasValue)
                query = query.Where(w => w.IsLiveIn == isLiveIn.Value);

            if (maxRate.HasValue)
                query = query.Where(w => w.MonthlyRate <= maxRate.Value || w.HourlyRate <= maxRate.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(w => w.User.FullName.Contains(search) ||
                                         (w.Bio != null && w.Bio.Contains(search)));

            if (nationalityId.HasValue)
                query = query.Where(w => w.NationalityId == nationalityId.Value);

            var totalCount = await query.CountAsync();
            var workers = await query
                .OrderByDescending(w => w.AverageRating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new
                {
                    w.Id,
                    w.User.FullName,
                    w.NationalityId,
                    w.WorkerSpecializationSpecs,
                    w.IsLiveIn,
                    w.MonthlyRate,
                    w.HourlyRate,
                    w.Currency,
                    w.CityId,
                    w.ExperienceYears,
                    w.AverageRating,
                    w.TotalReviews,
                    w.User.ProfileImageUrl,
                    w.Languages
                })
                .ToListAsync();

            return Ok(new
            {
                Data = workers,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWorker(int id)
        {
            var worker = await _context.WorkerProfiles
                .Include(w => w.User)
                .Include(w => w.Documents)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (worker == null)
                return NotFound();

            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.RevieweeId == worker.UserId && r.IsVisible)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    ReviewerName = r.Reviewer.FullName,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                worker.Id,
                worker.User.FullName,
                worker.NationalityId,
                worker.NationalIdNumber,
                worker.PassportNumber,
                worker.PassportCountry,
                worker.Bio,
                worker.ExperienceYears,
                worker.WorkerSpecializationSpecs,
                worker.IsLiveIn,
                worker.MonthlyRate,
                worker.HourlyRate,
                worker.Currency,
                worker.CityId,
                worker.Address,
                worker.AverageRating,
                worker.TotalReviews,
                worker.IsAvailable,
                worker.User.ProfileImageUrl,
                worker.Languages,
                Reviews = reviews
            });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _context.WorkerProfiles
                .Include(p => p.User)
                .Include(p => p.Documents)
                .Include(p => p.WorkerSpecializationSpecs)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound();

            return Ok(new
            {
                profile.Id,
                profile.User.FullName,
                profile.User.Email,
                profile.User.PhoneNumber,

                profile.NationalityId,
                profile.NationalIdNumber,
                profile.PassportNumber,
                profile.PassportExpiryDate,
                profile.PassportCountry,

                profile.CountryId,
                profile.StateId,
                profile.CityId,
                profile.Address,

                profile.Bio,
                profile.ExperienceYears,
                profile.PreviousEmployer,
                profile.WorkerSpecializationSpecs,
                profile.IsLiveIn,
                profile.IsAvailable,

                profile.MonthlyRate,
                profile.HourlyRate,
                profile.Currency,

                profile.AverageRating,
                profile.TotalReviews,
                profile.Languages,
                profile.VerificationStatus,

                // Matches the shape the Angular component expects for
                // selfieUrl()/passportUrl(): documents[].type / .documentImageUrl
                Documents = profile.Documents.Select(d => new
                {
                    d.Type,
                    d.DocumentImageUrl,
                    d.VerificationStatus
                })
            });

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var bookings = await _context.Bookings
                .Include(b => b.Homeowner)
                .Where(b => b.WorkerId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.Id,
                    HomeownerName = b.Homeowner.FullName,
                    b.ServiceType,
                    b.StartDate,
                    b.EndDate,
                    b.MonthlySalary,
                    b.CommissionAmount,
                    b.CommissionType,
                    b.Status,
                    b.IsPaid,
                    b.CreatedAt
                })
                .ToListAsync();

            return Ok(new { Data = bookings });
        }

        /// <summary>
        /// Single multipart/form-data request: the "profile" form field carries the
        /// JSON payload (everything the Angular reactive form holds, including the
        /// specialization rows) and the optional "selfieImage" / "passportImage"
        /// form files carry the two document photos. Everything is persisted in one
        /// call, matching what WorkerProfileComponent.saveProfile() now sends.
        /// </summary>
        [HttpPut("profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile(
            [FromForm] string profile,
            IFormFile? selfieImage,
            IFormFile? passportImage)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            UpdateWorkerProfileDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<UpdateWorkerProfileDto>(profile, JsonOptions);
            }
            catch (JsonException)
            {
                return BadRequest(new { Message = "بيانات الملف الشخصي غير صالحة" });
            }

            if (dto == null) return BadRequest(new { Message = "بيانات الملف الشخصي غير صالحة" });

            var workerProfile = await _context.WorkerProfiles
                .Include(p => p.Documents)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (workerProfile == null) return NotFound();

            // -- basic / identity info --
            workerProfile.NationalityId = dto.NationalityId ?? workerProfile.NationalityId;
            workerProfile.NationalIdNumber = dto.NationalIdNumber ?? workerProfile.NationalIdNumber;
            workerProfile.PassportNumber = dto.PassportNumber ?? workerProfile.PassportNumber;
            workerProfile.PassportExpiryDate = dto.PassportExpiryDate ?? workerProfile.PassportExpiryDate;
            workerProfile.PassportCountry = dto.PassportCountry ?? workerProfile.PassportCountry;

            // -- location --
            workerProfile.CountryId = dto.CountryId ?? workerProfile.CountryId;
            workerProfile.StateId = dto.StateId ?? workerProfile.StateId;
            workerProfile.CityId = dto.CityId ?? workerProfile.CityId;
            workerProfile.Address = dto.Address ?? workerProfile.Address;

            // -- professional info --
            workerProfile.Bio = dto.Bio ?? workerProfile.Bio;
            workerProfile.ExperienceYears = dto.ExperienceYears ?? workerProfile.ExperienceYears;
            workerProfile.Languages = dto.Languages ?? workerProfile.Languages;
            workerProfile.PreviousEmployer = dto.PreviousEmployer ?? workerProfile.PreviousEmployer;
            workerProfile.IsLiveIn = dto.IsLiveIn ?? workerProfile.IsLiveIn;
            workerProfile.IsAvailable = dto.IsAvailable ?? workerProfile.IsAvailable;

            // -- financial info --
            workerProfile.HourlyRate = dto.HourlyRate ?? workerProfile.HourlyRate;
            workerProfile.MonthlyRate = dto.MonthlyRate ?? workerProfile.MonthlyRate;
            workerProfile.Currency = dto.Currency ?? workerProfile.Currency;

            workerProfile.UpdatedAt = DateTime.UtcNow;

            // -- specializations: the frontend sends the whole desired list every
            // time (add/remove rows client-side), so replace the stored set --
            if (dto.WorkerSpecializationSpecs != null)
            {
                var existingSpecs = await _context.Set<WorkerSpecializationSpec>()
                    .Where(s => s.WorkerProfileId == workerProfile.Id)
                    .ToListAsync();
                _context.RemoveRange(existingSpecs);

                foreach (var specialization in dto.WorkerSpecializationSpecs
                             .Select(s => s.WorkerSpecialization)
                             .Distinct())
                {
                    _context.Add(new WorkerSpecializationSpec
                    {
                        WorkerProfileId = workerProfile.Id,
                        WorkerSpecialization = specialization
                    });
                }
            }

            // -- documents (only touched if the user actually picked a new file) --
            try
            {
                if (selfieImage != null)
                {
                    var url = await SaveDocumentFileAsync(selfieImage, userId);
                    UpsertDocument(workerProfile, DocumentType.Selfie, url);
                }

                if (passportImage != null)
                {
                    var url = await SaveDocumentFileAsync(passportImage, userId);
                    UpsertDocument(workerProfile, DocumentType.Passport, url);
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
            

            return Ok(new { Message = "تم تحديث الملف الشخصي بنجاح" });
        }

        private async Task<string> SaveDocumentFileAsync(IFormFile file, string userId)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
                throw new InvalidOperationException("صيغة الصورة غير مدعومة");

            if (file.Length > MaxDocumentSizeBytes)
                throw new InvalidOperationException("حجم الصورة أكبر من الحد المسموح به (5 ميجابايت)");

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "worker-documents");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Served by the static files middleware (app.UseStaticFiles()) from wwwroot.
            // Switch this to your blob/CDN URL builder if documents aren't stored on disk.
            return $"/uploads/worker-documents/{fileName}";
        }

        // NOTE: adjust the property names below (WorkerProfileId / Type /
        // DocumentImageUrl / VerificationStatus) if they differ from your actual
        // WorkerDocument entity - only its TypeScript shape was shared with me,
        // not the C# entity definition.
        private void UpsertDocument(WorkerProfile workerProfile, DocumentType type, string url)
        {
            var existing = workerProfile.Documents.FirstOrDefault(d => d.Type == type);
            if (existing != null)
            {
                existing.DocumentImageUrl = url;
                existing.VerificationStatus = VerificationStatus.Pending;
            }
            else
            {
                _context.Add(new WorkerDocument
                {
                    WorkerId = workerProfile.Id,
                    Type = type,
                    DocumentImageUrl = url,
                    VerificationStatus = VerificationStatus.Pending
                });
            }
        }
    }

    public class UpdateWorkerProfileDto
    {
        public int? NationalityId { get; set; }
        public string? NationalIdNumber { get; set; }
        public string? PassportNumber { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string? PassportCountry { get; set; }

        public int? CountryId { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }
        public string? Address { get; set; }

        public string? Bio { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Languages { get; set; }
        public string? PreviousEmployer { get; set; }
        public bool? IsLiveIn { get; set; }
        public bool? IsAvailable { get; set; }

        public decimal? HourlyRate { get; set; }
        public decimal? MonthlyRate { get; set; }
        public int? Currency { get; set; }

        public List<WorkerSpecializationSpecDto>? WorkerSpecializationSpecs { get; set; }
    }

    public class WorkerSpecializationSpecDto
    {
        public Specialization WorkerSpecialization { get; set; }
    }
}
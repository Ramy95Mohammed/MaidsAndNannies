using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Worker.Commands.UpdateProfile;
using MaidsAndNannies.Application.Features.Worker.Queries.GetMyBookings;
using MaidsAndNannies.Application.Features.Worker.Queries.GetMyProfile;
using MaidsAndNannies.Application.Features.Worker.Queries.GetWorkerById;
using MaidsAndNannies.Application.Features.Worker.Queries.GetWorkers;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MaidsAndNannies.WebApi.Controllers
{
    [Authorize]
    public sealed class WorkerController(ISender sender, ICurrentUserService currentUser) : BaseApiController
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

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
            var result = await sender.Send(new GetWorkersQuery(
                cityId, specialization, isLiveIn, maxRate, nationalityId, search, page, pageSize));
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWorker(int id)
        {
            var worker = await sender.Send(new GetWorkerByIdQuery(id));
            return Ok(worker);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();

            return Ok(await sender.Send(new GetMyWorkerProfileQuery(currentUser.UserId)));
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();

            var bookings = await sender.Send(new GetMyBookingsQuery(currentUser.UserId));
            return Ok(new { Data = bookings });
        }

        /// <summary>
        /// Single multipart/form-data request: the "profile" form field carries the
        /// JSON payload and the optional "selfieImage" / "passportImage" form files
        /// carry the two document photos.
        /// </summary>
        [HttpPut("profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile(
            [FromForm] string profile,
            IFormFile? selfieImage,
            IFormFile? passportImage)
        {
            if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();

            UpdateWorkerProfileRequest? dto;
            try
            {
                dto = JsonSerializer.Deserialize<UpdateWorkerProfileRequest>(profile, JsonOptions);
            }
            catch (JsonException)
            {
                return BadRequest(new { Message = "بيانات الملف الشخصي غير صالحة" });
            }

            if (dto is null) return BadRequest(new { Message = "بيانات الملف الشخصي غير صالحة" });

            var command = new UpdateWorkerProfileCommand(
                currentUser.UserId,
                dto.NationalityId, dto.NationalIdNumber, dto.WhatsAppNumber,
                dto.PassportNumber, dto.PassportExpiryDate, dto.PassportCountry,
                dto.CountryId, dto.StateId, dto.CityId, dto.Address,
                dto.Bio, dto.ExperienceYears, dto.Languages, dto.PreviousEmployer,
                dto.IsLiveIn, dto.IsAvailable, dto.HourlyRate, dto.MonthlyRate, dto.Currency,
                dto.WorkerSpecializationSpecs?.Select(s => s.WorkerSpecialization).ToList(),
                selfieImage is { Length: > 0 } ? selfieImage.OpenReadStream() : null,
                selfieImage?.FileName,
                passportImage is { Length: > 0 } ? passportImage.OpenReadStream() : null,
                passportImage?.FileName);

            await sender.Send(command);

            return Ok(new { Message = "تم تحديث الملف الشخصي بنجاح" });
        }

        /// <summary>
        /// The DB/Application layer only knows about relative URLs
        /// ("/uploads/worker-documents/x.jpg"). Turning that into an absolute URL
        /// the SPA can load is purely an HTTP concern, so it stays here.
        /// </summary>
        
    }

    public sealed class UpdateWorkerProfileRequest
    {
        public int? NationalityId { get; set; }
        public string? NationalIdNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
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

        public List<WorkerSpecializationSpecRequest>? WorkerSpecializationSpecs { get; set; }
    }

    public sealed class WorkerSpecializationSpecRequest
    {
        public Specialization WorkerSpecialization { get; set; }
    }
}

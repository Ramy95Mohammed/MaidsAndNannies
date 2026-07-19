using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MaidsAndNannies.Infrastructure.Persistence;
using MaidsPlatform.API.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MaidsAndNannies.WebApi.Controllers;

public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IConfiguration configuration) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var role = request.AccountType == AccountType.Worker ? UserRole.Worker : UserRole.Homeowner;
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = request.PreferredLanguage,
            Role = role
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors.Select(error => error.Description));

        await userManager.AddToRoleAsync(user, role.ToString());
        if (role == UserRole.Worker)
        {
            dbContext.WorkerProfiles.Add(new WorkerProfile
            {
                UserId = user.Id,
                NationalityId = request.NationalityId,
                CityId = request.CurrentCityId
            });
            await dbContext.SaveChangesAsync();
        }

        return Ok(CreateResponse(user, role.ToString()));
    }

    [HttpPost("register/worker")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> RegisterWorker([FromForm] RegisterWorkerRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { message = "كلمتا المرور غير متطابقتين" });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = "ar",
            Role = UserRole.Worker
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(" | ", result.Errors.Select(e => e.Description)) });

        await userManager.AddToRoleAsync(user, UserRole.Worker.ToString());

        var workerProfile = new WorkerProfile
        {
            UserId = user.Id,
            NationalityId = request.NationalityId,                        
            CountryId = request.CountryId,
            StateId = request.StateId,                        
            Bio = request.Bio ?? string.Empty,
            ExperienceYears = request.ExperienceYears,
            MonthlyRate = request.MonthlyRate,                      
            VerificationStatus = VerificationStatus.Pending
        };

        var selfieKey = await SaveImageAsync(request.SelfieImage, user.Id, "selfie");
        if (selfieKey is not null)
        {
            workerProfile.Documents = new List<WorkerDocument>
        {
            new() { Type = DocumentType.Selfie,  DocumentImageUrl = selfieKey }
        };
        }

        dbContext.WorkerProfiles.Add(workerProfile);
        await dbContext.SaveChangesAsync();

        return Ok(new { message = "تم التسجيل بنجاح، سيتم مراجعة حسابك." });
    }

    //private static async Task<string?> SaveFileAsync(IFormFile? file, string userId, string label)
    //{
    //    if (file is null || file.Length == 0) return null;

    //    var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "workers", userId);
    //    Directory.CreateDirectory(uploadsRoot);

    //    var fileName = $"{label}{Path.GetExtension(file.FileName)}";
    //    var fullPath = Path.Combine(uploadsRoot, fileName);

    //    await using var stream = new FileStream(fullPath, FileMode.Create);
    //    await file.CopyToAsync(stream);

    //    return $"/uploads/workers/{userId}/{fileName}";
    //}
   
    private static async Task<string?> SaveImageAsync(IFormFile? file, string userId, string label)
{
    if (file is null || file.Length == 0) return null;

    var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "workers", userId);
    Directory.CreateDirectory(uploadsRoot);

    var fileName = $"{label}{Path.GetExtension(file.FileName)}";
    var fullPath = Path.Combine(uploadsRoot, fileName);

    await using var stream = new FileStream(fullPath, FileMode.Create);
    await file.CopyToAsync(stream);

    return $"/uploads/workers/{userId}/{fileName}";
}

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);
        return Ok(CreateResponse(user, roles.Single()));
    }

    private AuthResponse CreateResponse(ApplicationUser user, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, role)
        };
        var expiresAt = DateTime.UtcNow.AddHours(8);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"], audience: configuration["Jwt:Audience"], claims: claims,
            expires: expiresAt, signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, user.FullName, role, user.PreferredLanguage , VerificationStatus.Pending);
    }
}

public enum AccountType { Homeowner, Worker }
public sealed class RegisterRequest
{
    [Required, StringLength(120)] public required string FullName { get; init; }
    [Required, EmailAddress] public required string Email { get; init; }
    [Required, Phone] public required string PhoneNumber { get; init; }
    [Required, MinLength(8)] public required string Password { get; init; }
    public AccountType AccountType { get; init; }
    [RegularExpression("^(ar|en)$")] public string PreferredLanguage { get; init; } = "ar";
    public int NationalityId { get; init; }
    public int CurrentCityId { get; init; }
}


public sealed class RegisterWorkerRequest
{
    [Required, StringLength(120)] public required string FullName { get; init; }
    [Required, EmailAddress] public required string Email { get; init; }
    [Required, Phone] public required string PhoneNumber { get; init; }
    [Required, MinLength(8)] public required string Password { get; init; }
    [Required] public required string ConfirmPassword { get; init; }
    public int NationalityId { get; init; }        
    public int? CountryId { get; init; }
    public int? StateId { get; init; } 
    public int ExperienceYears { get; init; }
    public decimal MonthlyRate { get; init; }
    public string? Bio { get; init; }    
    public IFormFile? SelfieImage { get; init; }
}

public sealed class LoginRequest { [Required, EmailAddress] public required string Email { get; init; } [Required] public required string Password { get; init; } }
public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, string FullName, string Role, string PreferredLanguage , VerificationStatus VerificationStatus);

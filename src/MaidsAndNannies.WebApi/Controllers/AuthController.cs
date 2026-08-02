using MaidsAndNannies.Application.Features.Auth.Commands.ChangePassword;
using MaidsAndNannies.Application.Features.Auth.Commands.ForgotPassword;
using MaidsAndNannies.Application.Features.Auth.Commands.Login;
using MaidsAndNannies.Application.Features.Auth.Commands.Register;
using MaidsAndNannies.Application.Features.Auth.Commands.RegisterHomeowner;
using MaidsAndNannies.Application.Features.Auth.Commands.RegisterWorker;
using MaidsAndNannies.Application.Features.Auth.Commands.ResetPassword;
using MaidsAndNannies.Application.Features.Auth.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MaidsAndNannies.WebApi.Controllers;

/// <summary>
/// Thin by design: every action just maps the HTTP request into a MediatR
/// Command and returns the result. All business rules (password matching,
/// role assignment, WorkerProfile seeding, JWT issuing...) live in
/// MaidsAndNannies.Application/Features/Auth.
/// </summary>
public sealed class AuthController(ISender sender) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.FullName, request.Email, request.PhoneNumber, request.Password,
            request.AccountType, request.PreferredLanguage, request.NationalityId, request.CurrentCityId);

        return Ok(await sender.Send(command));
    }

    [HttpPost("register/homeowner")]
    public async Task<ActionResult> RegisterHomeowner(RegisterHomeownerRequest request)
    {
        var command = new RegisterHomeownerCommand(
            request.FullName, request.Email, request.PhoneNumber, request.Password,
            request.City, request.Address);

        await sender.Send(command);
        return Ok(new { message = "تم التسجيل بنجاح، سيتم مراجعة حسابك." });
    }

    [HttpPost("register/worker")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> RegisterWorker([FromForm] RegisterWorkerRequest request)
    {
        Stream? selfieStream = null;
        if (request.SelfieImage is { Length: > 0 })
            selfieStream = request.SelfieImage.OpenReadStream();

        var command = new RegisterWorkerCommand(
            request.FullName, request.Email, request.PhoneNumber, request.Password, request.ConfirmPassword,
            request.NationalityId, request.CountryId, request.StateId, request.ExperienceYears,
            request.MonthlyRate, request.Bio, selfieStream, request.SelfieImage?.FileName);

        await sender.Send(command);
        return Ok(new { message = "تم التسجيل بنجاح، سيتم مراجعة حسابك." });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequest request)
    {
        return Ok(await sender.Send(new LoginCommand(request.Email, request.Password)));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordRequest request)
    {
        await sender.Send(new ChangePasswordCommand(request.CurrentPassword, request.NewPassword));
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        await sender.Send(new ForgotPasswordCommand(request.Email));
        return Ok(new { message = "إذا كان البريد مسجلًا، سيصلك كود الاستعادة عبر الواتساب من فريق الدعم." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordRequest request)
    {
        await sender.Send(new ResetPasswordCommand(request.Email, request.Code, request.NewPassword));
        return Ok(new { message = "تم تعيين كلمة المرور الجديدة بنجاح." });
    }
}

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

public sealed class RegisterHomeownerRequest
{
    [Required, StringLength(120)] public required string FullName { get; init; }
    [Required, EmailAddress] public required string Email { get; init; }
    [Required, Phone] public required string PhoneNumber { get; init; }
    [Required, MinLength(8)] public required string Password { get; init; }
    public string? City { get; init; }
    public string? Address { get; init; }
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

public sealed class LoginRequest
{
    [Required, EmailAddress] public required string Email { get; init; }
    [Required] public required string Password { get; init; }
}

public sealed class ChangePasswordRequest
{
    [Required] public required string CurrentPassword { get; init; }
    [Required, MinLength(8)] public required string NewPassword { get; init; }
}


public sealed class ForgotPasswordRequest
{
    [Required, EmailAddress] public required string Email { get; init; }
}

public sealed class ResetPasswordRequest
{
    [Required, EmailAddress] public required string Email { get; init; }
    [Required, StringLength(6, MinimumLength = 6)] public required string Code { get; init; }
    [Required, MinLength(8)] public required string NewPassword { get; init; }
}
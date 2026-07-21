using MaidsAndNannies.Application.Features.Auth.Commands.Login;
using MaidsAndNannies.Application.Features.Auth.Commands.Register;
using MaidsAndNannies.Application.Features.Auth.Commands.RegisterHomeowner;
using MaidsAndNannies.Application.Features.Auth.Commands.RegisterWorker;
using MaidsAndNannies.Application.Features.Auth.Common;
using MediatR;
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

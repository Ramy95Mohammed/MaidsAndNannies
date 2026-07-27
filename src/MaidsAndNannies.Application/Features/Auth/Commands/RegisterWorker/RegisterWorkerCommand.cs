using MediatR;

namespace MaidsAndNannies.Application.Features.Auth.Commands.RegisterWorker;

/// <summary>
/// SelfieImage is passed as a raw stream + file name so the Application layer
/// never needs a reference to IFormFile / ASP.NET Core (kept purely in the
/// WebApi mapping step).
/// </summary>
public sealed record RegisterWorkerCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string ConfirmPassword,
    int NationalityId,
    int? CountryId,
    int? StateId,
    int ExperienceYears,
    decimal MonthlyRate,
    string? Bio,
    Stream? SelfieImageContent,
    string? SelfieImageFileName) : IRequest<Unit>;

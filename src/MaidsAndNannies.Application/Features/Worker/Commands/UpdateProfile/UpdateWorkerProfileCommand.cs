using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.Worker.Commands.UpdateProfile;

public sealed record UpdateWorkerProfileCommand(
    string UserId,
    int? NationalityId,
    string? NationalIdNumber,
    DateTime BirthDate,
    string? WhatsAppNumber,
    string? PassportNumber,
    DateTime? PassportExpiryDate,
    string? PassportCountry,
    int? CountryId,
    int? StateId,
    int? CityId,
    string? Address,
    string? Bio,
    int? ExperienceYears,
    string? Languages,
    string? PreviousEmployer,
    bool? IsLiveIn,
    bool? IsAvailable,
    decimal? HourlyRate,
    decimal? MonthlyRate,
    int? Currency,
    IReadOnlyList<Specialization>? Specializations,
    Stream? SelfieImageContent,
    string? SelfieImageFileName,
    Stream? PassportImageContent,
    string? PassportImageFileName) : IRequest<Unit>;

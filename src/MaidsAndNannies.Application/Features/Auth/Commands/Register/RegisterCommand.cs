using MaidsAndNannies.Application.Features.Auth.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    AccountType AccountType,
    string PreferredLanguage,
    int NationalityId,
    int CurrentCityId) : IRequest<AuthResponseDto>;

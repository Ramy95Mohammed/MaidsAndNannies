using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Commands.RegisterHomeowner;

public sealed record AdminRegisterHomeownerCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string NationalIdNumber,
    string City,
    string Address,
    string AdminUserId) : IRequest<string>;
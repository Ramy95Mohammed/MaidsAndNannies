using MediatR;

namespace MaidsAndNannies.Application.Features.Homeowner.Commands.UpdateProfile;

public sealed record UpdateHomeownerProfileCommand(
    string UserId,
    string FullName,
    string WhatsAppNumber,
    string PhoneNumber,
    string? Address,
    string State,
    string? City,
    string? District,
    string? NationalIdNumber,
    Stream? NationalIdImageContent,
    string? NationalIdImageFileName,
    Stream? SelfieImageContent,
    string? SelfieImageFileName,
    Stream? ProofOfAddressImageContent,
    string? ProofOfAddressImageFileName) : IRequest<Unit>;
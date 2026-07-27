using MediatR;

namespace MaidsAndNannies.Application.Features.Auth.Commands.RegisterHomeowner;

/// <summary>
/// No document images for now - homeowners are being onboarded manually through
/// a trusted WhatsApp/Facebook network, so identity verification happens
/// out-of-band before an Admin approves the account. City/Address are optional
/// so we can still match bookings once they're filled in from the profile page.
/// </summary>
public sealed record RegisterHomeownerCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string? City,
    string? Address) : IRequest<Unit>;

namespace MaidsAndNannies.Application.Common.Interfaces;

/// <summary>
/// Implemented in the WebApi project (reads ClaimsPrincipal from HttpContext).
/// Handlers depend on this instead of ClaimsPrincipal/HttpContext directly, so
/// they stay framework-agnostic and unit-testable.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Role { get; }
}

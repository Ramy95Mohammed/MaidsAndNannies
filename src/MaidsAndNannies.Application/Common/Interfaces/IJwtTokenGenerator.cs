using MaidsAndNannies.Domain.Entities.Identity;

namespace MaidsAndNannies.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAtUtc) GenerateToken(ApplicationUser user, string role);
}

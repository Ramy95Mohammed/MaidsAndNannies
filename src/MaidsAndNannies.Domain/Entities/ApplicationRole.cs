using Microsoft.AspNetCore.Identity;

namespace MaidsAndNannies.Domain.Entities;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}

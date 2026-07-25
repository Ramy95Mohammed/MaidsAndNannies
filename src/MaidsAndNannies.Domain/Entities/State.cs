using MaidsAndNannies.Domain.Common;

namespace MaidsAndNannies.Domain.Entities;

public class State : Entity
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Iso2 { get; set; }
    public bool IsActive { get; set; } = true;

    public Country Country { get; set; } = null!;
}
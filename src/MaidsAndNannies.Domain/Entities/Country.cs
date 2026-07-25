using MaidsAndNannies.Domain.Common;

namespace MaidsAndNannies.Domain.Entities;

public class Country : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string Iso2 { get; set; } = string.Empty;
    public string Iso3 { get; set; } = string.Empty;
    public string? PhoneCode { get; set; }
    public string? Nationality { get; set; }
    public string? NationalityAr { get; set; }
    public string? CurrencyCode { get; set; }
    public bool IsActive { get; set; } = true;
}
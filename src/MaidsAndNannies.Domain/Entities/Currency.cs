using MaidsAndNannies.Domain.Common;

namespace MaidsAndNannies.Domain.Entities;

public class Currency : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal RateToEgp { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
}

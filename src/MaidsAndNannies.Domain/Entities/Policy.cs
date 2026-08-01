using MaidsAndNannies.Domain.Common;

namespace MaidsAndNannies.Domain.Entities;

public sealed class Policy : Entity
{
    public string Key { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string ContentAr { get; set; } = string.Empty;
    public string ContentEn { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
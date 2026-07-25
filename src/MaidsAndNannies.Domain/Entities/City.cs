using MaidsAndNannies.Domain.Common;

namespace MaidsAndNannies.Domain.Entities;

public class City : Entity
{
    public int StateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool IsActive { get; set; } = true;

    public State State { get; set; } = null!;
}
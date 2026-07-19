namespace MaidsAndNannies.Domain.Common;

public abstract class Entity
{
    public int Id { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}

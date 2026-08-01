using MaidsAndNannies.Domain.Entities;

namespace MaidsAndNannies.Application.Common.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(string userId, string type, string titleKey, object? payload = null, CancellationToken ct = default);
    Task NotifyAdminsAsync(string type, string titleKey, object? payload = null, CancellationToken ct = default);
}
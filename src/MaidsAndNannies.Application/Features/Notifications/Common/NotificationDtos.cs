namespace MaidsAndNannies.Application.Features.Notifications.Common;

public sealed record NotificationDto(
    int Id, string Type, string Title, string Message, bool IsRead, DateTime CreatedAt);
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Notifications.Commands.MarkAllNotificationsRead;
using MaidsAndNannies.Application.Features.Notifications.Commands.MarkNotificationRead;
using MaidsAndNannies.Application.Features.Notifications.Queries.GetMyNotifications;
using MaidsAndNannies.Application.Features.Notifications.Queries.GetUnreadNotificationsCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaidsAndNannies.WebApi.Controllers;

[Authorize]
public sealed class NotificationsController(ISender sender, ICurrentUserService currentUser) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(await sender.Send(new GetMyNotificationsQuery(currentUser.UserId)));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(new { Count = await sender.Send(new GetUnreadNotificationsCountQuery(currentUser.UserId)) });
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new MarkNotificationReadCommand(currentUser.UserId, id));
        return Ok(new { Message = "تم التحديد كمقروء" });
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new MarkAllNotificationsReadCommand(currentUser.UserId));
        return Ok(new { Message = "تم تحديد الكل كمقروء" });
    }
}
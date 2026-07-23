using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Subscription.Commands.Admin.ConfirmSubscriptionRenewal;
using MaidsAndNannies.Application.Features.Subscription.Commands.RenewSubscription;
using MaidsAndNannies.Application.Features.Subscription.Common;
using MaidsAndNannies.Application.Features.Subscription.Queries.GetAllSubscriptions;
using MaidsAndNannies.Application.Features.Subscription.Queries.GetMySubscriptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaidsAndNannies.WebApi.Controllers;

public sealed class SubscriptionController(ISender sender, ICurrentUserService currentUser) : BaseApiController
{
    [HttpGet("my")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> GetMySubscriptions()
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(await sender.Send(new GetMySubscriptionsQuery(currentUser.UserId)));
    }

    [HttpPost("{id}/renew")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> Renew(int id, [FromForm] RenewSubscriptionRequest request, IFormFile? proofImage)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();

        await sender.Send(new RenewSubscriptionCommand(
            id, currentUser.UserId, request.PaymentMethod, request.Amount,
            request.TransactionReference,
            proofImage?.OpenReadStream(), proofImage?.FileName));

        return Ok(new { Message = "تم إرسال طلب التجديد" });
    }

    // ── Admin ──

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllSubscriptions()
        => Ok(await sender.Send(new GetAllSubscriptionsQuery()));

    [HttpPost("{id}/confirm")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConfirmRenewal(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new ConfirmSubscriptionRenewalCommand(id, currentUser.UserId));
        return Ok(new { Message = "تم تأكيد تجديد الاشتراك" });
    }
}
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Admin.Commands.ApproveHomeowner;
using MaidsAndNannies.Application.Features.Admin.Commands.ApproveWorker;
using MaidsAndNannies.Application.Features.Admin.Commands.ConfirmPayment;
using MaidsAndNannies.Application.Features.Admin.Commands.RejectHomeowner;
using MaidsAndNannies.Application.Features.Admin.Commands.RejectPayment;
using MaidsAndNannies.Application.Features.Admin.Commands.RejectWorker;
using MaidsAndNannies.Application.Features.Admin.Queries.GetAdminDashboard;
using MaidsAndNannies.Application.Features.Admin.Queries.GetAllBookings;
using MaidsAndNannies.Application.Features.Admin.Queries.GetPendingHomeowners;
using MaidsAndNannies.Application.Features.Admin.Queries.GetPendingPayments;
using MaidsAndNannies.Application.Features.Admin.Queries.GetPendingWorkers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MaidsAndNannies.WebApi.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdminController(ISender sender, ICurrentUserService currentUser) : BaseApiController
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
        => Ok(await sender.Send(new GetAdminDashboardQuery()));

    [HttpGet("workers/pending")]
    public async Task<IActionResult> GetPendingWorkers()
        => Ok(await sender.Send(new GetPendingWorkersQuery()));

    [HttpGet("homeowners/pending")]
    public async Task<IActionResult> GetPendingHomeowners()
        => Ok(await sender.Send(new GetPendingHomeownersQuery()));

    [HttpPost("workers/{id}/approve")]
    public async Task<IActionResult> ApproveWorker(int id)
    {
        await sender.Send(new ApproveWorkerCommand(id, currentUser.UserId!));
        return Ok(new { Message = "تم اعتماد العاملة بنجاح" });
    }

    [HttpPost("workers/{id}/reject")]
    public async Task<IActionResult> RejectWorker(int id, RejectRequest request)
    {
        await sender.Send(new RejectWorkerCommand(id, currentUser.UserId!, request.Reason));
        return Ok(new { Message = "تم رفض العاملة" });
    }

    [HttpPost("homeowners/{id}/approve")]
    public async Task<IActionResult> ApproveHomeowner(int id)
    {
        await sender.Send(new ApproveHomeownerCommand(id, currentUser.UserId!));
        return Ok(new { Message = "تم اعتماد صاحبة المنزل بنجاح" });
    }

    [HttpPost("homeowners/{id}/reject")]
    public async Task<IActionResult> RejectHomeowner(int id, RejectRequest request)
    {
        await sender.Send(new RejectHomeownerCommand(id, currentUser.UserId!, request.Reason));
        return Ok(new { Message = "تم رفض صاحبة المنزل" });
    }

    // ── Payments ──

    [HttpGet("payments/pending")]
    public async Task<IActionResult> GetPendingPayments()
        => Ok(await sender.Send(new GetPendingPaymentsQuery()));

    [HttpPost("payments/{id}/confirm")]
    public async Task<IActionResult> ConfirmPayment(int id)
    {
        await sender.Send(new ConfirmPaymentCommand(id, currentUser.UserId!));
        return Ok(new { Message = "تم تأكيد الدفع" });
    }

    [HttpPost("payments/{id}/reject")]
    public async Task<IActionResult> RejectPayment(int id, RejectRequest request)
    {
        await sender.Send(new RejectPaymentCommand(id, request.Reason));
        return Ok(new { Message = "تم رفض الدفع" });
    }

    // ── Bookings ──

    [HttpGet("bookings")]
    public async Task<IActionResult> GetAllBookings()
    => Ok(await sender.Send(new GetAllBookingsQuery()));
}

public sealed class RejectRequest
{
    [Required] public required string Reason { get; init; }
}
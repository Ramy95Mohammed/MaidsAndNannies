using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Commands.Admin;
using MaidsAndNannies.Application.Features.Bookings.Commands.CancelBooking;
using MaidsAndNannies.Application.Features.Bookings.Commands.CreateBooking;
using MaidsAndNannies.Application.Features.Bookings.Commands.RequestReplacement;
using MaidsAndNannies.Application.Features.Bookings.Commands.UploadPaymentProof;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Application.Features.Bookings.Queries.GetBookingById;
using MaidsAndNannies.Application.Features.Bookings.Queries.GetMyBookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaidsAndNannies.Application.Features.Reviews.Commands.ReviewBooking;

namespace MaidsAndNannies.WebApi.Controllers;

public sealed class BookingController(ISender sender, ICurrentUserService currentUser) : BaseApiController
{
    // ── Homeowner ──

    [HttpPost]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> Create(CreateBookingRequest request)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();

        var bookingDetailDto = await sender.Send(new CreateBookingCommand(
            currentUser.UserId, request.WorkerId, request.ServiceType,
            request.BookingType,request.Quantity, request.StartDate, request.MonthlySalary ,request.DailySalary , request.HourlySalary, request.CommissionType));

        return Ok(new { BookingId = bookingDetailDto.Id, Message = "تم إنشاء الحجز بنجاح" });
    }


    [HttpPost("bookingCreationInfo")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> GetBookingCreationInfo(CreateBookingRequest request)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();

        var bookingInfo = await
            sender.Send(new CreateBookingCommand(
            currentUser.UserId, request.WorkerId, request.ServiceType,
            request.BookingType, request.Quantity, request.StartDate, request.MonthlySalary, request.DailySalary, request.HourlySalary, request.CommissionType,true));

        return Ok(bookingInfo);
    }

    [HttpGet]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> GetMyBookings(
     [FromQuery] int? status = null,
     [FromQuery] int? bookingType = null,
     [FromQuery] DateTime? fromDate = null,
     [FromQuery] DateTime? toDate = null,
     [FromQuery] string? search = null,
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(await sender.Send(new GetMyBookingsQuery(currentUser.UserId, currentUser.Role ?? "", status, bookingType, fromDate, toDate, search, page, pageSize)));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(await sender.Send(new GetBookingByIdQuery(id, currentUser.UserId, currentUser.Role ?? "")));
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> Cancel(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new CancelBookingCommand(id, currentUser.UserId));
        return Ok(new { Message = "تم إلغاء الحجز" });
    }

    [HttpPost("{id}/upload-proof")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> UploadProof(int id, [FromForm] UploadPaymentProofRequest request,
        IFormFile? proofImage)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();

        await sender.Send(new UploadPaymentProofCommand(
            id, currentUser.UserId, request.PaymentMethod, request.Amount,
            request.CommissionAmount,
            request.TransactionReference,
            proofImage?.OpenReadStream(), proofImage?.FileName));

        return Ok(new { Message = "تم رفع إثبات الدفع" });
    }

    [HttpPost("{id}/replace")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> Replace(int id, [FromBody] ReplaceBookingRequest request)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();

        await sender.Send(new RequestReplacementCommand(
            id, currentUser.UserId, request.NewWorkerId, request.ApplicationId, request.Reason));
        return Ok(new { Message = "تم طلب الاستبدال" });
    }

    // ── Worker ──

    [HttpGet("worker")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> GetWorkerBookings(
    [FromQuery] int? status = null,
    [FromQuery] int? bookingType = null,
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null,
    [FromQuery] string? search = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(await sender.Send(new GetMyBookingsQuery(currentUser.UserId, currentUser.Role ?? "", status, bookingType, fromDate, toDate, search, page, pageSize)));
    }

    // ── Admin ──

    [HttpPost("{id}/confirm-worker")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConfirmWorker(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new ConfirmWorkerCommand(id, currentUser.UserId));
        return Ok(new { Message = "تم تأكيد العاملة" });
    }

    [HttpPost("{id}/request-payment")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RequestPayment(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new RequestPaymentCommand(id, currentUser.UserId));
        return Ok(new { Message = "تم طلب الدفع من صاحبة المنزل" });
    }

    [HttpPost("{id}/confirm-payment")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConfirmPayment(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new ConfirmPaymentCommand(id, currentUser.UserId));
        return Ok(new { Message = "تم تأكيد الدفع" });
    }

    [HttpPost("{id}/start")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> StartWork(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new StartWorkCommand(id, currentUser.UserId));
        return Ok(new { Message = "تم بدء العمل" });
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CompleteWork(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new CompleteWorkCommand(id, currentUser.UserId));
        return Ok(new { Message = "تم إنهاء الحجز" });
    }

    // ── تقييم بعد إتمام الحجز (صاحبة منزل أو عاملة) ──

    [HttpPost("{id}/review")]
    [Authorize(Roles = "Homeowner,Worker")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewBookingRequest request)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new ReviewBookingCommand(id, currentUser.UserId, request.Rating, request.Comment));
        return Ok(new { Message = "تم إرسال التقييم بنجاح" });
    }
}

public sealed record ReplaceBookingRequest(
    int? NewWorkerId,
    int? ApplicationId,
    MaidsAndNannies.Domain.Enums.ReplacementReason Reason);

public sealed record ReviewBookingRequest(int Rating, string? Comment);
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Application.Features.JobPost.Queries.GetJobPostCalculation;
using MaidsAndNannies.Application.Features.JobPosts.Commands.AcceptApplication;
using MaidsAndNannies.Application.Features.JobPosts.Commands.ApplyForJob;
using MaidsAndNannies.Application.Features.JobPosts.Commands.CreateJobPost;
using MaidsAndNannies.Application.Features.JobPosts.Commands.DeleteJobPost;
using MaidsAndNannies.Application.Features.JobPosts.Commands.UpdateJobPost;
using MaidsAndNannies.Application.Features.JobPosts.Queries.GetApprovedJobPosts;
using MaidsAndNannies.Application.Features.JobPosts.Queries.GetJobApplications;
using MaidsAndNannies.Application.Features.JobPosts.Queries.GetJobPostById;
using MaidsAndNannies.Application.Features.JobPosts.Queries.GetMyJobApplications;
using MaidsAndNannies.Application.Features.JobPosts.Queries.GetMyJobPosts;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaidsAndNannies.WebApi.Controllers;

[Authorize]
public sealed class JobPostsController(ISender sender, ICurrentUserService currentUser) : BaseApiController
{
    [HttpPost]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> Create([FromBody] CreateJobPostRequest request)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        var id = await sender.Send(new CreateJobPostCommand(
            currentUser.UserId, request.Description, request.MonthlySalary,
            request.DailySalary, request.HourlySalary, request.Specialization,
            request.BookingType, request.CommissionType, request.StartDate, request.Quantity, request.CurrencyId,
            request.Specializations));
        return Ok(new { JobPostId = id, Message = "تم إنشاء الإعلان بانتظار مراجعة الإدارة" });
    }


    [HttpPost("jobPostCalculationInfo")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> GetJobPostCalculationInfo(CreateJobPostRequest request)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();

        var bookingInfo = await
            sender.Send(new CalculateJobPostCommissionQuery(request.BookingType, request.Quantity , request.CommissionType,
            request.MonthlySalary , request.DailySalary , request.HourlySalary , request.CurrencyId));

        return Ok(bookingInfo);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> GetMyPosts()
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(await sender.Send(new GetMyJobPostsQuery(currentUser.UserId)));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(await sender.Send(new GetJobPostByIdQuery(id, currentUser.UserId, currentUser.Role ?? "")));
    }

    [HttpGet]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> GetApproved(
       [FromQuery] int? specialization = null,
       [FromQuery] int? bookingType = null,
       [FromQuery] decimal? minMonthlySalary = null,
       [FromQuery] decimal? maxMonthlySalary = null,
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 12)
    {
        return Ok(await sender.Send(new GetApprovedJobPostsQuery(specialization, bookingType, minMonthlySalary, maxMonthlySalary, page, pageSize)));
    }

    [HttpPost("{id}/apply")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> Apply(int id, [FromBody] ApplyRequest? request)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new ApplyForJobCommand(id, currentUser.UserId, request?.Message));
        return Ok(new { Message = "تم تقديم الطلب بنجاح" });
    }

    [HttpGet("{id}/applications")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> GetApplications(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(await sender.Send(new GetJobApplicationsQuery(id, currentUser.UserId)));
    }

    [HttpPost("{postId}/applications/{appId}/accept")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> AcceptApplication(int postId, int appId)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        var bookingId = await sender.Send(new AcceptApplicationCommand(postId, appId, currentUser.UserId));
        return Ok(new { BookingId = bookingId, Message = "تم قبول الطلب وإنشاء الحجز" });
    }

    [HttpGet("my-applications")]
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> GetMyApplications()
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(await sender.Send(new GetMyJobApplicationsQuery(currentUser.UserId)));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateJobPostRequest request)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new UpdateJobPostCommand(
            id, currentUser.UserId, request.Description, request.MonthlySalary,
            request.DailySalary, request.HourlySalary, request.Specialization,
            request.BookingType, request.CommissionType, request.StartDate, request.Quantity, request.CurrencyId,
            request.Specializations));
        return Ok(new { Message = "تم تحديث الإعلان بانتظار مراجعة الإدارة" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Homeowner")]
    public async Task<IActionResult> Delete(int id)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        await sender.Send(new DeleteJobPostCommand(id, currentUser.UserId));
        return Ok(new { Message = "تم حذف الإعلان" });
    }
}

public sealed record CreateJobPostRequest(
    string Description, decimal MonthlySalary, decimal DailySalary,
    decimal HourlySalary, Specialization Specialization, BookingType BookingType,
    CommissionType CommissionType, DateTime StartDate, int Quantity, int CurrencyId,
    List<Specialization>? Specializations = null);

public sealed record ApplyRequest(string? Message);
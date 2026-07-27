using MaidsAndNannies.Application.Features.JobPosts.Commands.ReviewJobPost;
using MaidsAndNannies.Application.Features.JobPosts.Queries.GetPendingJobPosts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaidsAndNannies.WebApi.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdminJobPostsController(ISender sender) : BaseApiController
{
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
        => Ok(await sender.Send(new GetPendingJobPostsQuery()));

    [HttpPut("{id}/review")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewJobPostRequest request)
    {
        await sender.Send(new ReviewJobPostCommand(
            id, request.SanitizedDescription, request.IsApproved, request.RejectionReason));
        var msg = request.IsApproved ? "تم اعتماد الإعلان" : "تم رفض الإعلان";
        return Ok(new { Message = msg });
    }
}

public sealed record ReviewJobPostRequest(
    string SanitizedDescription, bool IsApproved, string? RejectionReason);
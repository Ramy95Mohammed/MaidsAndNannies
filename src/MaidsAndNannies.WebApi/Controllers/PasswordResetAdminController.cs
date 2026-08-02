using MaidsAndNannies.Application.Features.Admin.Commands.MarkResetRequestSent;
using MaidsAndNannies.Application.Features.Admin.Queries.GetPasswordResetRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaidsAndNannies.WebApi.Controllers;

[ApiController]
[Route("api/admin/password-reset")]
[Authorize(Roles = "Admin")]
public sealed class PasswordResetAdminController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetRequests(CancellationToken ct)
    {
        return Ok(await sender.Send(new GetPasswordResetRequestsQuery(), ct));
    }

    [HttpPost("{id}/mark-sent")]
    public async Task<ActionResult> MarkSent(int id, CancellationToken ct)
    {
        await sender.Send(new MarkResetRequestSentCommand(id), ct);
        return NoContent();
    }
}
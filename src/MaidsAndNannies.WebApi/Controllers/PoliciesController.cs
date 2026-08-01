using MaidsAndNannies.Application.Features.Policies.Commands.UpdatePolicy;
using MaidsAndNannies.Application.Features.Policies.Queries.GetPolicies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MaidsAndNannies.WebApi.Controllers;

public sealed class PoliciesController(ISender sender) : BaseApiController
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult> GetPolicies(CancellationToken ct)
    {
        return Ok(await sender.Send(new GetPoliciesQuery(), ct));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{key}")]
    public async Task<ActionResult> UpdatePolicy(string key, UpdatePolicyRequest request, CancellationToken ct)
    {
        await sender.Send(new UpdatePolicyCommand(
            key, request.TitleAr, request.TitleEn, request.ContentAr, request.ContentEn,
            request.SortOrder, request.IsActive), ct);
        return NoContent();
    }
}

public sealed class UpdatePolicyRequest
{
    [Required] public required string TitleAr { get; init; }
    [Required] public required string TitleEn { get; init; }
    [Required] public required string ContentAr { get; init; }
    [Required] public required string ContentEn { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}
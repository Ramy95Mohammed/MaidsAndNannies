using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Homeowner.Commands.UpdateProfile;
using MaidsAndNannies.Application.Features.Homeowner.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaidsAndNannies.WebApi.Controllers;

[Authorize(Roles = "Homeowner")]
public sealed class HomeownerController(ISender sender, ICurrentUserService currentUser) : BaseApiController
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();
        return Ok(await sender.Send(new GetMyHomeownerProfileQuery(currentUser.UserId)));
    }

    [HttpPut("profile")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateHomeownerProfileRequest request,
        IFormFile? nationalIdImage,
        IFormFile? selfieImage,
        IFormFile? proofOfAddressImage)
    {
        if (string.IsNullOrEmpty(currentUser.UserId)) return Unauthorized();

        var command = new UpdateHomeownerProfileCommand(
            currentUser.UserId,
            request.FullName,
            request.WhatsAppNumber,
            request.PhoneNumber,
            request.Address, request.State,request.City, request.District, request.NationalIdNumber,
            nationalIdImage?.OpenReadStream(), nationalIdImage?.FileName,
            selfieImage?.OpenReadStream(), selfieImage?.FileName,
            proofOfAddressImage?.OpenReadStream(), proofOfAddressImage?.FileName);

        await sender.Send(command);
        return Ok(new { Message = "تم تحديث الملف الشخصي بنجاح" });
    }
}

public sealed class UpdateHomeownerProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? District { get; set; }
    public string? NationalIdNumber { get; set; }
}
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.AdminSettings.Commands.UpdateSettings;
using MaidsAndNannies.Application.Features.AdminSettings.Queries.GetSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaidsAndNannies.WebApi.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdminSettingsController(ISender sender, ICurrentUserService currentUser)
    : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await sender.Send(new GetSettingsQuery()));

    [HttpPut]
    public async Task<IActionResult> Update(List<UpdateSettingItem> settings)
    {
        await sender.Send(new UpdateSettingsCommand(settings, currentUser.UserId!));
        return Ok(new { Message = "تم حفظ الإعدادات" });
    }
}
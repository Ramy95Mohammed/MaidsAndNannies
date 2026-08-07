using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.AdminSettings.Commands.UpdateSettings;
using MaidsAndNannies.Application.Features.AdminSettings.Queries.GetSettingByKey;
using MaidsAndNannies.Application.Features.AdminSettings.Queries.GetSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaidsAndNannies.WebApi.Controllers;


public sealed class AdminSettingsController(ISender sender, ICurrentUserService currentUser)
    : BaseApiController
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await sender.Send(new GetSettingsQuery()));


    [Authorize(Roles = "Admin,Homeowner")]
    [HttpGet("{key}")]
    public async Task<IActionResult> GetSettingByKey(string key)
       => Ok(await sender.Send(new GetSettingByKey(key)));

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> Update(List<UpdateSettingItem> settings)
    {
        await sender.Send(new UpdateSettingsCommand(settings, currentUser.UserId!));
        return Ok(new { Message = "تم حفظ الإعدادات" });
    }
}
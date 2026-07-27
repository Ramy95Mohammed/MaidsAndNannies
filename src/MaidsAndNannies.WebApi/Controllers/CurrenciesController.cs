using MaidsAndNannies.Application.Features.Currency.Commands.CreateCurrency;
using MaidsAndNannies.Application.Features.Currency.Commands.DeleteCurrency;
using MaidsAndNannies.Application.Features.Currency.Commands.UpdateCurrency;
using MaidsAndNannies.Application.Features.Currency.Common;
using MaidsAndNannies.Application.Features.Currency.Queries.GetCurrencies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaidsAndNannies.WebApi.Controllers;

public sealed class CurrenciesController(ISender sender) : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        => Ok(await sender.Send(new GetCurrenciesQuery(includeInactive)));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateCurrencyRequest request)
    {
        var id = await sender.Send(new CreateCurrencyCommand(
            request.Code, request.Symbol, request.NameAr, request.NameEn,
            request.RateToEgp, request.IsActive));
        return Ok(new { CurrencyId = id, Message = "تم إضافة العملة بنجاح" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateCurrencyRequest request)
    {
        await sender.Send(new UpdateCurrencyCommand(
            id, request.Code, request.Symbol, request.NameAr, request.NameEn,
            request.RateToEgp, request.IsActive));
        return Ok(new { Message = "تم تحديث العملة بنجاح" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await sender.Send(new DeleteCurrencyCommand(id));
        return Ok(new { Message = "تم حذف العملة بنجاح" });
    }
}
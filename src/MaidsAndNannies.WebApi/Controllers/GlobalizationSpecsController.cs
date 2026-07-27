using MaidsAndNannies.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.WebApi.Controllers;

public sealed class GlobalizationSpecsController(IApplicationDbContext dbContext) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult> GetCountries()
        => Ok(await dbContext.Countries.OrderBy(c => c.Name_en).ToListAsync());

    [HttpGet("{countryId}")]
    public async Task<ActionResult> GetStatesByCountryId(int countryId)
        => Ok(await dbContext.States.Where(s => s.Country_id == countryId).OrderBy(s => s.Name_en).ToListAsync());

    [HttpGet("stats/{stateId}")]
    public async Task<ActionResult> GetCitiesByStateId(int stateId)
        => Ok(await dbContext.Cities.Where(c => c.State_id == stateId).OrderBy(c => c.Name_en).ToListAsync());
}
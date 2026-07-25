using MaidsAndNannies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;

namespace MaidsAndNannies.Infrastructure.Persistence;

public sealed class GeoSeeder
{
    private readonly IServiceProvider _services;
    private readonly IHostEnvironment _environment;

    public GeoSeeder(IServiceProvider services, IHostEnvironment environment) { _services = services; _environment = environment; }

    public async Task SeedAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        var rootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");

        if (!await db.Countries.AnyAsync())
        {
            var countriesJsonPath = Path.Combine(rootPath, "json", "Countries.json");
            var countriesJson = await File.ReadAllTextAsync(countriesJsonPath);

            var countriesList = JsonSerializer.Deserialize<List<Country>>(countriesJson);
            

            await db.Countries.AddRangeAsync(countriesList);
            await db.SaveChangesAsync();
        }

        // ── States ──
        if (!await db.States.AnyAsync())
        {
            var statesJsonPath = Path.Combine(rootPath, "json", "States.json");
            var statesJson = await File.ReadAllTextAsync(statesJsonPath);
            var statesList = JsonSerializer.Deserialize<List<State>>(statesJson);
            await db.States.AddRangeAsync(statesList);
            await db.SaveChangesAsync();
        }

        // ── Citiees ──
        if (!await db.Cities.AnyAsync())
        {
            var citiesJsonPath = Path.Combine(rootPath, "json" , "Cities.json");
            var citiesJson = await File.ReadAllTextAsync(citiesJsonPath);
            var citiesList = JsonSerializer.Deserialize<List<City>>(citiesJson);
            await db.Cities.AddRangeAsync(citiesList);
            await db.SaveChangesAsync();
        }
    }    
}
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

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var rootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");

        // ── Countries ──
        if (!await db.Countries.AnyAsync())
        {
            var countriesJsonPath = Path.Combine(
                rootPath,
                "json",
                "Countries.json");

            var countriesJson = await File.ReadAllTextAsync(countriesJsonPath);

            var countriesList =
                JsonSerializer.Deserialize<List<Country>>(countriesJson)
                ?? new List<Country>();

            foreach (var batch in countriesList.Chunk(20))
            {
                await db.Countries.AddRangeAsync(batch);
                await db.SaveChangesAsync();
            }
        }

        // ── States ──
        if (!await db.States.AnyAsync())
        {
            var statesJsonPath = Path.Combine(
                rootPath,
                "json",
                "States.json");

            var statesJson = await File.ReadAllTextAsync(statesJsonPath);

            var statesList =
                JsonSerializer.Deserialize<List<State>>(statesJson)
                ?? new List<State>();

            foreach (var batch in statesList.Chunk(50))
            {
                await db.States.AddRangeAsync(batch);
                await db.SaveChangesAsync();
            }
        }

        // ── Cities ──
        //if (!await db.Cities.AnyAsync())
        //{
        //    var citiesJsonPath = Path.Combine(
        //        rootPath,
        //        "json",
        //        "Cities.json");

        //    var citiesJson = await File.ReadAllTextAsync(citiesJsonPath);

        //    var citiesList =
        //        JsonSerializer.Deserialize<List<City>>(citiesJson)
        //        ?? new List<City>();

        //    foreach (var batch in citiesList.Chunk(100))
        //    {
        //        await db.Cities.AddRangeAsync(batch);
        //        await db.SaveChangesAsync();
        //    }
        //}
    }
}
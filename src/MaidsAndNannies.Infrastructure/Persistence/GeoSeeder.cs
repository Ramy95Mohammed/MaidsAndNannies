using MaidsAndNannies.Application.Dtos;
using MaidsAndNannies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace MaidsAndNannies.Infrastructure.Persistence;

public sealed class GeoSeeder
{
    private readonly IServiceProvider _services;

    public GeoSeeder(IServiceProvider services) => _services = services;

    public async Task SeedAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

        if (!await db.Countries.AnyAsync())
        {
            var res = await client.GetFromJsonAsync<CountriesResponse>(
                "https://csc.sidsworld.co.in/api/countries");
            if (res?.Countries is null) return;

            foreach (var c in res.Countries)
            {
                db.Countries.Add(new Country
                {
                    Id = c.Id,
                    Name = c.Name ?? "",
                    NameAr = c.Native,          // <-- native → NameAr
                    Iso2 = c.Iso2 ?? "",
                    Iso3 = c.Iso3 ?? "",
                    PhoneCode = c.PhoneCode,
                    Nationality = c.Nationality,
                    NationalityAr = GetNationalityAr(c.Nationality), // <-- ترجمة يدوية
                    CurrencyCode = c.Currency,
                });
            }
            await db.SaveChangesAsync();
        }

        // ── States ──
        if (!await db.States.AnyAsync())
        {
            var countryIds = await db.Countries.Select(c => c.Id).ToListAsync();
            foreach (var cid in countryIds)
            {
                try
                {
                    var res = await client.GetFromJsonAsync<StatesResponse>(
                        $"https://csc.sidsworld.co.in/api/states/{cid}");
                    if (res?.States is null) continue;
                    foreach (var s in res.States)
                        db.States.Add(new State { Id = s.Id, CountryId = cid, Name = s.Name ?? "" });
                }
                catch { continue; }
                if (countryIds.IndexOf(cid) % 20 == 0) await db.SaveChangesAsync();
            }
            await db.SaveChangesAsync();
        }

        // ── Citiees ──
        if (!await db.Cities.AnyAsync())
        {
            var satetesIds = await db.States.Select(c => c.Id).ToListAsync();
            foreach (var sid in satetesIds)
            {
                try
                {
                    var res = await client.GetFromJsonAsync<CitiesResponseDto>(
                        $"https://csc.sidsworld.co.in/api/cities/{sid}");
                    if (res?.Cities is null) continue;
                    foreach (var s in res.Cities)
                        db.Cities.Add(new City { Id = s.Id, StateId = sid, Name = s.Name ?? "" });
                }
                catch { continue; }
                if (satetesIds.IndexOf(sid) % 20 == 0) await db.SaveChangesAsync();
            }
            await db.SaveChangesAsync();
        }
    }

    private static string? GetNationalityAr(string? en)
    {
        return en switch
        {
            "Egyptian" => "مصري",
            "Saudi" => "سعودي",
            "Emirati" => "إماراتي",
            "Kuwaiti" => "كويتي",
            "Qatari" => "قطري",
            "Bahraini" => "بحرييني",
            "Omani" => "عماني",
            "Jordanian" => "أردني",
            "Lebanese" => "لبناني",
            "Syrian" => "سوري",
            "Palestinian" => "فلسطيني",
            "Iraqi" => "عراقي",
            "Yemeni" => "يمني",
            "Libyan" => "ليبي",
            "Tunisian" => "تونسي",
            "Algerian" => "جزائري",
            "Moroccan" => "مغربي",
            "Sudanese" => "سوداني",
            "Somali" => "صومالي",
            "Pakistani" => "باكستاني",
            "Indian" => "هندي",
            "Bangladeshi" => "بنغلاديشي",
            "Filipino" => "فلبيني",
            "Indonesian" => "أندونيسي",
            "Sri Lankan" => "سيريلانكي",
            "Nepalese" => "نيبالي",
            "Chinese" => "صيني",
            "Turkish" => "تركي",
            "Ethiopian" => "إثيوبي",
            "Eritrean" => "إريتري",
            "Kenyan" => "كيني",
            "Nigerian" => "نيجيري",
            "Ghanaian" => "غاني",
            "South African" => "جنوب أفريقي",
            "American" => "أمريكي",
            "British" => "بريطاني",
            "French" => "فرنسي",
            "German" => "ألماني",
            "Italian" => "إيطالي",
            "Spanish" => "إسباني",
            "Canadian" => "كندي",
            "Australian" => "أسترالي",
            _ => null
        };
    }

    private sealed record CountriesResponse(int Status, List<CountryEntry> Countries);
    private sealed record CountryEntry(int Id, string? Name, string? Native, string? Iso2, string? Iso3, string? PhoneCode, string? Nationality, string? Currency);
    private sealed record StatesResponse(int Status, List<StateEntry> States);
    private sealed record StateEntry(int Id, string? Name);
}
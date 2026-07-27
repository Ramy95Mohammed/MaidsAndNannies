using MaidsAndNannies.Domain.Common;
using System.Text.Json.Serialization;

namespace MaidsAndNannies.Domain.Entities;

public class Country
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("name_en")]
    public string? Name_en { get; set; }
    [JsonPropertyName("name_ar")]
    public string? Name_ar { get; set; }
    [JsonPropertyName("nationality_en")]
    public string? Nationality_en { get; set; }
    [JsonPropertyName("nationality_ar")]
    public string? Nationality_ar { get; set; }
    [JsonPropertyName("iso2")]
    public string? Iso2 { get; set; }
    [JsonPropertyName("iso3")]
    public string? Iso3 { get; set; }
    [JsonPropertyName("capital")]
    public string? Capital { get; set; }
    [JsonPropertyName("capital_ar")]
    public string? Capital_ar { get; set; }
    [JsonPropertyName("region")]
    public string? Region { get; set; }
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    [JsonPropertyName("phone_code")]
    public string? Phone_code { get; set; }    
}
using MaidsAndNannies.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MaidsAndNannies.Domain.Entities;

public class City
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("name_en")]
    public string? Name_en { get; set; }
    [JsonPropertyName("name_ar")]
    public string? Name_ar { get; set; }
    [JsonPropertyName("latitude")]
    public string? Latitude { get; set; }
    [JsonPropertyName("longitude")]
    public string? Longitude { get; set; }
    [JsonPropertyName("state_id")]
    public int? State_id { get; set; }
    public State State { get; set; } = null!;
    [JsonPropertyName("country_id")]
    public int? Country_id { get; set; }
    public Country Country { get; set; } = null!;
}
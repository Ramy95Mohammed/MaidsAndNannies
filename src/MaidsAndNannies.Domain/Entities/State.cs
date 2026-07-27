using MaidsAndNannies.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MaidsAndNannies.Domain.Entities;

public class State 
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("name_en")]
    public string? Name_en { get; set; }
    [JsonPropertyName("name_ar")]
    public string? Name_ar { get; set; }
    [JsonPropertyName("state_code")]
    public string? State_code { get; set; }
    [JsonPropertyName("country_id")]
    public int? Country_id { get; set; }
    public Country Country { get; set; } = null!;
}
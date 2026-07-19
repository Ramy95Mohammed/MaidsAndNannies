using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaidsAndNannies.Application.Dtos
{
    using System.Text.Json.Serialization;

    public class StateDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("country_id")]
        public int? CountryId { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("fips_code")]
        public string? FipsCode { get; set; }

        [JsonPropertyName("iso2")]
        public string? Iso2 { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("latitude")]
        public string? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public string? Longitude { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("flag")]
        public int? Flag { get; set; }

        [JsonPropertyName("wikiDataId")]
        public string? WikiDataId { get; set; }
    }

    public class StatesResponseDto
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("states")]
        public List<StateDto> States { get; set; } = [];
    }
}

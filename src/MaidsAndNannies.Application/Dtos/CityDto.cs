using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaidsAndNannies.Application.Dtos
{
    using System.Text.Json.Serialization;

    public class CityDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("state_id")]
        public int? StateId { get; set; }

        [JsonPropertyName("state_code")]
        public string? StateCode { get; set; }

        [JsonPropertyName("country_id")]
        public int? CountryId { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }

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

    public class CitiesResponseDto
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("cities")]
        public List<CityDto> Cities { get; set; } = [];
    }
}

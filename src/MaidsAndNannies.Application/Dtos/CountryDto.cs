using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaidsAndNannies.Application.Dtos
{
    using System.Text.Json.Serialization;

    public class CountryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("iso3")]
        public string? Iso3 { get; set; }

        [JsonPropertyName("numeric_code")]
        public string? NumericCode { get; set; }

        [JsonPropertyName("iso2")]
        public string? Iso2 { get; set; }

        [JsonPropertyName("phonecode")]
        public string? PhoneCode { get; set; }

        [JsonPropertyName("capital")]
        public string? Capital { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("currency_name")]
        public string? CurrencyName { get; set; }

        [JsonPropertyName("currency_symbol")]
        public string? CurrencySymbol { get; set; }

        [JsonPropertyName("tld")]
        public string? Tld { get; set; }

        [JsonPropertyName("native")]
        public string? Native { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("region_id")]
        public int? RegionId { get; set; }

        [JsonPropertyName("subregion")]
        public string? Subregion { get; set; }

        [JsonPropertyName("subregion_id")]
        public int? SubregionId { get; set; }

        [JsonPropertyName("nationality")]
        public string? Nationality { get; set; }      

        [JsonPropertyName("latitude")]
        public string? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public string? Longitude { get; set; }

        [JsonPropertyName("emoji")]
        public string? Emoji { get; set; }

        [JsonPropertyName("emojiU")]
        public string? EmojiU { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("flag")]
        public int? Flag { get; set; }

        [JsonPropertyName("wikiDataId")]
        public string? WikiDataId { get; set; }
    }

    public class CountriesResponseDto
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("countries")]
        public List<CountryDto> Countries { get; set; } = [];
    }
}

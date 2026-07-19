using MaidsAndNannies.Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace MaidsAndNannies.WebApi.Controllers
{   
    public class GlobalizationSpecsController : BaseApiController
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GlobalizationSpecsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<ActionResult<List<CountryDto>>> GetCountries()
        {
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://csc.sidsworld.co.in/api/countries");
           
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var countries = await response.Content.ReadFromJsonAsync<CountriesResponseDto>();
            return Ok(countries.Countries);
        }


        [HttpGet("{countryId}")]
        public async Task<ActionResult<List<StateDto>>> GetStatesByCountryId(int countryId)
        {
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://csc.sidsworld.co.in/api/states/" + countryId);
            
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var states = await response.Content.ReadFromJsonAsync<StatesResponseDto>();
            return Ok(states.States);
        }


        [HttpGet("stats/{stateId}")]
        public async Task<ActionResult<List<StateDto>>> GetCitiesByStateId(int stateId)
        {
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://csc.sidsworld.co.in/api/cities/" + stateId);

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var cities = await response.Content.ReadFromJsonAsync<CitiesResponseDto>();
            return Ok(cities.Cities);
        }





    }
}

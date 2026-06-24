using WeatherForecast.Web.Api.Models;
using WeatherForecast.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace WeatherForecast.Web.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly HttpClient _client;
        public const string BasePath = "/api/find";

        public WeatherForecastService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<IEnumerable<WeatherForecastModel>> Find()
        {
            var response = await _client.GetAsync(BasePath);

            return await response.Content.ReadFromJsonAsync<List<WeatherForecastModel>>() ?? new List<WeatherForecastModel>();
        }
    }
}

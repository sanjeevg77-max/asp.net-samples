using System.Text.Json;
using WeatherForecast.Web.Api.Models;
using WeatherForecast.Web.Services.Interfaces;

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

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<WeatherForecastModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }
}

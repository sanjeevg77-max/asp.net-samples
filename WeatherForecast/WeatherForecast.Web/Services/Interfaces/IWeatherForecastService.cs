using WeatherForecast.Web.Api.Models;

namespace WeatherForecast.Web.Services.Interfaces
{
    public interface IWeatherForecastService
    {
        Task<IEnumerable<WeatherForecastModel>> Find();
    }
}

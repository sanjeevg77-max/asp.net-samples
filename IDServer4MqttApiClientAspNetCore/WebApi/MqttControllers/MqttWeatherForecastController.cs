using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Threading.Tasks;

namespace WebApi.MqttControllers
{
    public class MqttWeatherForecastController
    {
        private readonly ILogger<MqttWeatherForecastController> _logger;

        // Controllers have full support for dependency injection just like AspNetCore controllers
        public MqttWeatherForecastController(ILogger<MqttWeatherForecastController> logger)
        {
            _logger = logger;
        }

        // Handles weather report messages for a given zip code on topic "{zipCode}/temperature"
        public Task WeatherReport(MqttApplicationMessageInterceptorContext context, int zipCode)
        {
            // We have access to the MqttContext
            if (zipCode != 90210)
            {
                context.AcceptPublish = false;
                return Task.CompletedTask;
            }

            // We have access to the raw message
            var temperature = BitConverter.ToDouble(context.ApplicationMessage.Payload.ToArray());

            _logger.LogInformation($"It's {temperature} degrees in Hollywood");

            // Example validation
            if (temperature <= 0 || temperature >= 130)
            {
                context.AcceptPublish = false;
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
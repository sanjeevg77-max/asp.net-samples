using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace MqttClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var rnd = new Random();

            var mqttFactory = new MqttFactory();
            var mqttClient = mqttFactory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId($"Client{rnd.Next(0, 1000)}")
                .WithWebSocketServer(o => o.WithUri("192.168.2.3:7201/mqtt"))
                .Build();

            mqttClient.ConnectedAsync += e =>
            {
                Console.WriteLine($"Connection Result: {e.ConnectResult.ResultCode}");
                return Task.CompletedTask;
            };

            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine($"Message from {e.ClientId}: {e.ApplicationMessage.PayloadSegment.Count} bytes.");
                return Task.CompletedTask;
            };

            // Setup and start the MQTT client.
            await mqttClient.ConnectAsync(options, CancellationToken.None);

            await mqttClient.SubscribeAsync("MqttWeatherForecast/90210/temperature");

            // Publish a message on a well known topic
            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic("MqttWeatherForecast/90210/temperature")
                .WithPayload(BitConverter.GetBytes(98.6d))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());

            // Publish a message on a topic the server doesn't explicitly handle
            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic("asdfsdfsadfasdf")
                .WithPayload(BitConverter.GetBytes(100d))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());

            // ConnectAsync returns after connecting; wait for user input before exiting.
            Console.ReadLine();

            await mqttClient.DisconnectAsync();
        }
    }
}

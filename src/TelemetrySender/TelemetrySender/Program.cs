using Azure;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace TelemetrySender
{
    class Program
    {
        private static readonly TransportType transportType = TransportType.Mqtt;
        private static DeviceClient deviceClient;

        private static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
              .Build();
            string iotHubConnectionString = configuration.GetValue<string>("IotHubConnectionString");

            deviceClient = DeviceClient.CreateFromConnectionString(iotHubConnectionString, transportType);
            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChangedAsync, null);

            IotHubConnectionStringBuilder iotHubConnectionStringBuilder = IotHubConnectionStringBuilder.Create(iotHubConnectionString);
            string deviceId = iotHubConnectionStringBuilder.DeviceId;
            
            Twin twin = await deviceClient.GetTwinAsync().ConfigureAwait(false);
            string country = twin.Properties.Desired["country"].ToString();
            string city = twin.Properties.Desired["city"].ToString();
            string eventHubNamespaceName = twin.Properties.Desired["eventHubNamespaceName"].ToString();
            string eventHubName = twin.Properties.Desired["eventHubName"].ToString();
            string sasToken = twin.Properties.Desired["sasToken"].ToString();

            TwinCollection reportedProperties = new ();
            reportedProperties["eventHubNamespaceName"] = eventHubNamespaceName;
            reportedProperties["eventHubName"] = eventHubName;
            reportedProperties["sasToken"] = sasToken;
            reportedProperties["DateTimeLastAppLaunch"] = DateTime.UtcNow;
            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

            EventHubProducerClient eventHubProducerClient = new (
                $"{eventHubNamespaceName}.servicebus.windows.net", 
                $"{eventHubName}/publishers/{deviceId}", 
                new AzureSasCredential(sasToken)
            );

            Console.WriteLine($"Device ID: {deviceId}");
            Console.WriteLine($"Country: {country}");
            Console.WriteLine($"City: {city}");
            Console.WriteLine("\nEnter Temperature");
            float temperature = Convert.ToSingle(Console.ReadLine());
            Console.WriteLine("\nEnter Humidity");
            float humidity = Convert.ToSingle(Console.ReadLine());

            WeatherData weatherData = new ()
            {
                DeviceId = deviceId,
                Country = country,
                City = city,
                Temperature = temperature,
                Humidity = humidity,
            };

            BinaryData eventBody = new (JsonConvert.SerializeObject(weatherData));
            EventData eventData = new (eventBody);

            using EventDataBatch eventBatch = await eventHubProducerClient.CreateBatchAsync();
            eventBatch.TryAdd(eventData);

            await eventHubProducerClient.SendAsync(eventBatch);

            Console.WriteLine($"Sent message: {JsonConvert.SerializeObject(weatherData, Formatting.Indented)}");
        }

        private static async Task OnDesiredPropertyChangedAsync(TwinCollection desiredProperties, object userContext)
        {
            TwinCollection reportedProperties = new ();
            reportedProperties["DateTimeLastDesiredPropertyChangeReceived"] = DateTime.UtcNow;
            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
        }
    }
}

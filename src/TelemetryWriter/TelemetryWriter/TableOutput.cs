using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using TelemetryWriter.Models;

namespace TelemetryWriter
{
    public static class TableOutput
    {
        [FunctionName("TableOutput")]
        public static async Task Run([EventHubTrigger("%EventHubName%", Connection = "EventHubConnectionString")] EventData[] events)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(messageBody);
                    string publisher = eventData.SystemProperties.PartitionKey;

                    DateTime currentTime = DateTime.UtcNow;
                    string rowKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - currentTime.Ticks);

                    if (publisher == weatherData.DeviceId)
                    {
                        var weatherTableEntity = new WeatherTableEntity(weatherData.City, rowKey)
                        {
                            Temperature = weatherData.Temperature.ToString(),
                            Humidity = weatherData.Humidity.ToString(),
                            DeviceId = weatherData.DeviceId,
                        };

                        CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("StorageConnectionString"));
                        CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
                        CloudTable cloudTable = cloudTableClient.GetTableReference(Environment.GetEnvironmentVariable("TableName"));
                        TableOperation insertMergeOperation = TableOperation.InsertOrMerge(weatherTableEntity);
                        TableResult result = await cloudTable.ExecuteAsync(insertMergeOperation);
                        WeatherTableEntity insertedWeatherData = result.Result as WeatherTableEntity;

                        Console.WriteLine($"Entity written in table: {JsonConvert.SerializeObject(insertedWeatherData, Formatting.Indented)}");
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}

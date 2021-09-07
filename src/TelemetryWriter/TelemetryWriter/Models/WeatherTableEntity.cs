using Microsoft.Azure.Cosmos.Table;

namespace TelemetryWriter.Models
{
    class WeatherTableEntity : TableEntity
    {
        public WeatherTableEntity(string country, string city)
        {
            PartitionKey = country;
            RowKey = city;
        }
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string DeviceId { get; set; }
    }
}

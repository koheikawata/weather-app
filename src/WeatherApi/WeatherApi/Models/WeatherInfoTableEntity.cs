using Microsoft.Azure.Cosmos.Table;
using System;

namespace WeatherApi.Models
{
    public class WeatherInfoTableEntity : TableEntity
    {
        public WeatherInfoTableEntity()
        {
        }
        public WeatherInfoTableEntity(string country, string city)
        {
            PartitionKey = country;
            RowKey = city;
        }
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string DeviceId { get; set; }
    }
}

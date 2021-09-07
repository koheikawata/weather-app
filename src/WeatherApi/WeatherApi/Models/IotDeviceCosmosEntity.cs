using Newtonsoft.Json;

namespace WeatherApi.Models
{
    public class IotDeviceCosmosEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
    }
}

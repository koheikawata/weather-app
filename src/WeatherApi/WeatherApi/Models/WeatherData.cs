using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherApi.Models
{
    public class WeatherData
    {
        public string DeviceId { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
    }
}

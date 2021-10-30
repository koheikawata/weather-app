using System;

namespace WeatherApi.Models
{
    public class WeatherHistory
    {
        public int Id { get; set; }
        public string City { get; set; }
        public string Weather { get; set; }
        public int Temperature { get; set; }
        public DateTime Date { get; set; }
    }
}

using System;
using System.Linq;
using WeatherApi.Models;

namespace WeatherApi.Data
{
    public class SqlDbInitializer
    {
        public static void Initialize(WeatherHistoryContext context)
        {
            context.Database.EnsureCreated();

            if (context.WeatherHistories.Any())
            {
                return;
            }

            WeatherHistory[] weatherHistory = new []
            {
                new WeatherHistory{City="Tokyo", Weather="Sunny", Temperature=10, Date=DateTime.Parse("2021-09-01")},
                new WeatherHistory{City="Paris", Weather="Rainy", Temperature=10, Date=DateTime.Parse("2021-09-01")},
                new WeatherHistory{City="Tokyo", Weather="Cloudy", Temperature=10, Date=DateTime.Parse("2021-09-02")},
                new WeatherHistory{City="London", Weather="Cloudy", Temperature=10, Date=DateTime.Parse("2021-09-02")},
                new WeatherHistory{City="London", Weather="Sunny", Temperature=10, Date=DateTime.Parse("2021-09-03")},
                new WeatherHistory{City="Paris", Weather="Snowy", Temperature=10, Date=DateTime.Parse("2021-09-03")},
                new WeatherHistory{City="Tokyo", Weather="Cloudy", Temperature=10, Date=DateTime.Parse("2021-09-04")},
                new WeatherHistory{City="Tokyo", Weather="Rainy", Temperature=10, Date=DateTime.Parse("2021-09-05")},
                new WeatherHistory{City="Tokyo", Weather="Rainy", Temperature=10, Date=DateTime.Parse("2021-09-06")},
            };
            foreach (WeatherHistory weather in weatherHistory)
            {
                context.WeatherHistories.Add(weather);
            }
            context.SaveChanges();
        }
    }
}

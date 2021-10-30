using Microsoft.EntityFrameworkCore;
using WeatherApi.Models;

namespace WeatherApi.Data
{
    public class WeatherHistoryContext : DbContext
    {
        public WeatherHistoryContext(DbContextOptions<WeatherHistoryContext> options) : base(options)
        {
        }

        public DbSet<WeatherHistory> WeatherHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherHistory>().ToTable("WeatherHistory");
        }
    }
}

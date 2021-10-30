using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherApi.Data;
using WeatherApi.Models;

namespace WeatherApi.Controllers
{
    [ApiController]
    public class WeatherHistoryController : ControllerBase
    {
        private readonly WeatherHistoryContext weatherHistoryContext;

        public WeatherHistoryController(WeatherHistoryContext weatherHistoryContext)
        {
            this.weatherHistoryContext = weatherHistoryContext;
        }

        [HttpGet("api/[controller]")]
        public async Task<ActionResult<IEnumerable<WeatherHistory>>> GetStudents()
        {
            return await this.weatherHistoryContext.WeatherHistories.ToListAsync();
        }
    }
}

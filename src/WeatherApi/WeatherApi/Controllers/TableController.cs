using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeatherApi.Interfaces;
using WeatherApi.Models;

namespace WeatherApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly CloudTableClient cloudTableClient;

        public TableController(IConfiguration configuration, CloudTableClient cloudTableClient)
        {
            this.configuration = configuration;
            this.cloudTableClient = cloudTableClient;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WeatherRequest weatherRequest)
        {
            string a = this.configuration.GetValue<string>("Storage:TableName");
            CloudTable cloudTable = this.cloudTableClient.GetTableReference(this.configuration.GetValue<string>("Storage:TableName"));
            string cityFilter = TableQuery.GenerateFilterCondition(nameof(WeatherInfoTableEntity.PartitionKey), QueryComparisons.Equal, weatherRequest.City);
            TableQuery<WeatherInfoTableEntity> cityQuery = new TableQuery<WeatherInfoTableEntity>().Where(cityFilter);
            TableQuerySegment<WeatherInfoTableEntity> cityQuerySegment = await cloudTable.ExecuteQuerySegmentedAsync(cityQuery, null);
            WeatherInfoTableEntity resultEntity = (from c in cityQuerySegment select c).FirstOrDefault();

            return this.Created("result", resultEntity);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherApi.Models;

namespace WeatherApi.Interfaces
{
    public interface ICosmosService
    {
        public Task CreateItemAsync(IotDeviceCosmosEntity iotDeviceCosmosEntity);
        public Task<List<IotDeviceCosmosEntity>> GetItemsAsync();
    }
}

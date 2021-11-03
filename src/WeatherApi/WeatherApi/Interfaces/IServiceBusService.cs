using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherApi.Interfaces
{
    public interface IServiceBusService
    {
        public Task InitializerAsync();
        public List<string> GetReceivedMessages();
    }
}

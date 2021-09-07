using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WeatherApi.Interfaces;
using WeatherApi.Models;

namespace WeatherApi.Services
{
    public class CosmosService : ICosmosService
    {
        private readonly Container container;

        public CosmosService(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            this.container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task CreateItemAsync(IotDeviceCosmosEntity iotDeviceCosmosEntity)
        {
            string thisDeviceId = null;
            while (thisDeviceId is null)
            {
                iotDeviceCosmosEntity.Id = Guid.NewGuid().ToString();

                try
                {
                    await this.container.ReadItemAsync<IotDeviceCosmosEntity>(iotDeviceCosmosEntity.Id, new PartitionKey(iotDeviceCosmosEntity.DeviceId));
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    thisDeviceId = iotDeviceCosmosEntity.Id;
                }
            }

            await this.container.CreateItemAsync<IotDeviceCosmosEntity>(iotDeviceCosmosEntity, new PartitionKey(iotDeviceCosmosEntity.DeviceId));
        }

        public async Task<List<IotDeviceCosmosEntity>> GetItemsAsync()
        {
            var query = this.container.GetItemQueryIterator<IotDeviceCosmosEntity>(new QueryDefinition("SELECT * FROM c"));
            List<IotDeviceCosmosEntity> results = new ();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }
    }
}

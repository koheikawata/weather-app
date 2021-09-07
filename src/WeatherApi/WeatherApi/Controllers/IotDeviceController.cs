using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WeatherApi.Interfaces;
using WeatherApi.Models;


namespace WeatherApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IotDeviceController : ControllerBase
    {
        private readonly IIothubService iothubService;
        private readonly ICosmosService cosmosService;
        private readonly IConfiguration configuration;

        public IotDeviceController(IIothubService iothubService, ICosmosService cosmosService, IConfiguration configuration)
        {
            this.iothubService = iothubService;
            this.cosmosService = cosmosService;
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] IotDevice iotDevice)
        {
            string deviceId = iotDevice.DeviceId;
            string eventHubNamespaceName = this.configuration.GetValue<string>("EventHubs:EventHubNamespaceName");
            string eventHubName = this.configuration.GetValue<string>("EventHubs:EventHubName");
            string sharedAccessPolicyName = this.configuration.GetValue<string>("EventHubs:SharedAccessPolicyName");
            string sharedAccessPolicyKey = this.configuration.GetValue<string>("EventHubs:SharedAccessPolicyKey");

            Device createdDevice = await this.iothubService.RegisterDeviceAsync(deviceId).ConfigureAwait(false);

            Twin twin = await this.iothubService.GetTwinAsync(deviceId).ConfigureAwait(false);
            Twin twinPatch = new ();
            twinPatch.Properties.Desired["country"] = iotDevice.Country;
            twinPatch.Properties.Desired["city"] = iotDevice.City;
            twinPatch.Properties.Desired["eventHubNamespaceName"] = eventHubNamespaceName;
            twinPatch.Properties.Desired["eventHubName"] = eventHubName;
            twinPatch.Properties.Desired["sasToken"] = CreateToken($"https://{eventHubNamespaceName}.servicebus.windows.net/{eventHubName}/publishers/{deviceId}", sharedAccessPolicyName, sharedAccessPolicyKey);

            await this.iothubService.UpdateTwinAsync(deviceId, twinPatch, twin.ETag);

            IotDeviceCosmosEntity iotDeviceCosmosEntity = new ()
            {
                DeviceId = iotDevice.DeviceId,
                Country = iotDevice.Country,
                City = iotDevice.City,
            };
            await this.cosmosService.CreateItemAsync(iotDeviceCosmosEntity);

            IotHubConnectionStringBuilder iotHubConnectionStringBuilder = IotHubConnectionStringBuilder.Create(this.configuration.GetValue<string>("IotHub:IotHubConnectionString"));
            string iotHubHostName = iotHubConnectionStringBuilder.HostName;
            string deviceConnectionString = "HostName=" + iotHubHostName + ";DeviceId=" + iotDevice.DeviceId + ";SharedAccessKey=" + createdDevice.Authentication.SymmetricKey.PrimaryKey;
            return this.Created("deviceinfo", new { connectionString = deviceConnectionString, deviceId = iotDevice.DeviceId, country = iotDevice.Country, city = iotDevice.City });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<IotDeviceCosmosEntity> listIotDeviceCosmosEntity = await this.cosmosService.GetItemsAsync();
            return Created("listdevice", listIotDeviceCosmosEntity);
        }

        private static string CreateToken(string resourceUri, string keyName, string key)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int year = 12 * 60 * 60 * 24 * 7;
            string expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + year);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new (Encoding.UTF8.GetBytes(key));
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            string sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }
    }
}

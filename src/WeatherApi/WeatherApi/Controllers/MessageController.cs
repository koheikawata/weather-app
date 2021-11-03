using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherApi.Interfaces;
using WeatherApi.Models;

namespace WeatherApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IConfiguration configuration; 
        private readonly ServiceBusClient serviceBusClient;
        private readonly IServiceBusService serviceBusService;

        public MessageController(IConfiguration configuration, ServiceBusClient serviceBusClient, IServiceBusService serviceBusService)
        {
            this.configuration = configuration;
            this.serviceBusClient = serviceBusClient;
            this.serviceBusService = serviceBusService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WeatherMessage weatherMessage)
        {
            ServiceBusSender sender = this.serviceBusClient.CreateSender(this.configuration.GetValue<string>("ServiceBus:QueueName"));
            ServiceBusMessage message = new (weatherMessage.Message);
            await sender.SendMessageAsync(message).ConfigureAwait(false);
            await sender.CloseAsync().ConfigureAwait(false);

            return this.Created("message", weatherMessage.Message);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string joinedMessages;

            await serviceBusService.InitializerAsync();
            List<string> messages = serviceBusService.GetReceivedMessages();
            if (messages.Count > 0)
            {
                joinedMessages = string.Join(Environment.NewLine, messages);
            }
            else
            {
                joinedMessages = "No messages arrived yet.";
            }

            return this.Created("joinedmessages", joinedMessages);
        }

    }
}

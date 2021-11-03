using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherApi.Interfaces;

namespace WeatherApi.Services
{
    public class ServiceBusService : IServiceBusService
    {
        private readonly IConfiguration configuration;
        private readonly ServiceBusClient serviceBusClient;
        private List<string> receivedMessages = new ();

        public ServiceBusService(IConfiguration configuration, ServiceBusClient serviceBusClient)
        {
            this.configuration = configuration;
            this.serviceBusClient = serviceBusClient;
        }

        public async Task InitializerAsync()
        {
            ServiceBusProcessor processor = this.serviceBusClient.CreateProcessor(this.configuration.GetValue<string>("ServiceBus:QueueName"));

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();
        }

        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            receivedMessages.Add(args.Message.Body.ToString());
            await args.CompleteMessageAsync(args.Message);
        }

        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            return Task.CompletedTask;
        }

        public List<string> GetReceivedMessages()
        {
            List<string> messages = new (this.receivedMessages);
            
            return messages;
        }
    }
}

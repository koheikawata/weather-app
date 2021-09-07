using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using WeatherApi.Interfaces;
using WeatherApi.Services;


namespace WeatherApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<RegistryManager>(sp =>
                RegistryManager.CreateFromConnectionString(this.Configuration.GetValue<string>("IotHub:IotHubConnectionString"))
            );
            services.AddSingleton<CloudTableClient>(InitializeCloudTableClientInstance(this.Configuration));
            services.AddSingleton<ICosmosService>(InitializeCosmosClientInstanceAsync(this.Configuration).GetAwaiter().GetResult());

            services.AddScoped<IIothubService, IothubService>();
            services.AddScoped<ITableService, TableServcie>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static async Task<CosmosService> InitializeCosmosClientInstanceAsync(IConfiguration configuration)
        {
            string account = configuration.GetValue<string>("CosmosDb:Account");
            string key = configuration.GetValue<string>("CosmosDb:Key");
            string databaseName = configuration.GetValue<string>("CosmosDb:DatabaseName");
            string containerName = configuration.GetValue<string>("CosmosDb:ContainerName");
            string partitionKey = configuration.GetValue<string>("CosmosDb:PartitionKey");
            CosmosClient cosmosClient = new (account, key);
            CosmosService cosmosService = new (cosmosClient, databaseName, containerName);
            DatabaseResponse database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/" + partitionKey);

            return cosmosService;
        }

        private static CloudTableClient InitializeCloudTableClientInstance(IConfiguration configuration)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(configuration.GetValue<string>("Storage:StorageConnectionString"));
            return cloudStorageAccount.CreateCloudTableClient();
        }
    }
}

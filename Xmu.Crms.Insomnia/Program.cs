using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Xmu.Crms.Shared;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Insomnia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .UseKestrel()
                .ConfigureServices(collection =>
                {
                    collection.AddSingleton(CreateClusterClient);

                    collection
                        .AddCrmsView("API.Insomnia")
                        ;
                })
                .UseStartup<Startup>();
        }

        private static IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
        {
            var client = new ClientBuilder()
                .UseLocalhostClustering(30000, "silo-insomnia", "insomnia")
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "silo-insomnia";
                    options.ServiceId = "insomnia";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IUserService).Assembly))
                .Build();

            StartClientWithRetries(client).Wait();
            return client;
        }

        private static async Task StartClientWithRetries(IClusterClient client)
        {
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    await client.Connect();
                    return;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
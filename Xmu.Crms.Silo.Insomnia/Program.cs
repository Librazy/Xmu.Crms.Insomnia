using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Xmu.Crms.Silo.Insomnia
{

    public class Program
    {
        private static ISiloHost _silo;
        private static readonly ManualResetEvent _SiloStopped = new ManualResetEvent(false);

        private static void Main()
        {
            _silo = new SiloHostBuilder()
                .UseLocalhostClustering(11111, 30000, null, "silo-insomnia")
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "silo-insomnia";
                    options.ServiceId = "insomnia";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(InsomniaExtensions).Assembly).WithReferences())
                .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddConsole())
                .Build();

            Task.Run(StartSilo);

            AssemblyLoadContext.Default.Unloading += context =>
            {
                Task.Run(StopSilo);
                _SiloStopped.WaitOne();
            };

            _SiloStopped.WaitOne();
        }

        private static async Task StartSilo()
        {
            await _silo.StartAsync();
            Console.WriteLine("Silo started");
        }

        private static async Task StopSilo()
        {
            await _silo.StopAsync();
            Console.WriteLine("Silo stopped");
            _SiloStopped.Set();
        }
    }
}

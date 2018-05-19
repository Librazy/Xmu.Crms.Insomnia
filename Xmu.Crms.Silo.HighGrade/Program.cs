using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Silo.HighGrade
{
    public class Program
    {
        private static ISiloHost _silo;
        private static readonly ManualResetEvent _SiloStopped = new ManualResetEvent(false);

        private static void Main()
        {
            _silo = new SiloHostBuilder()
                .UseLocalhostClustering(11111, 30000, null, "silo-highgrade")
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "silo-highgrade";
                    options.ServiceId = "highgrade";
                })
                .ConfigureServices(svc => svc.AddDbContextPool<CrmsContext>(options =>
                    options.UseMySQL("Server=localhost;Database=crmsdb;Uid=root;Pwd=root;SslMode=none")
                ))
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HighGradeExtensions).Assembly).WithReferences())
                .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Information).AddConsole())
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


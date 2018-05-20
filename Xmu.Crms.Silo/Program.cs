using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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

namespace Xmu.Crms.Silo
{
    public class Program
    {
        private static ISiloHost _silo;
        private static readonly ManualResetEvent _SiloStopped = new ManualResetEvent(false);

        private static void Main()
        {
            _silo = new SiloHostBuilder()
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = "MySql.Data.MySqlClient";
                    options.ConnectionString = "Server=localhost;Database=crmsdb;Uid=root;Pwd=root;SslMode=none";
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "xmu.crms.silo";
                    options.ServiceId = "xmu.crms.silo.services";
                })
                .ConfigureEndpoints(GetLocalIpAddress(), 11111, 30000)
                .ConfigureServices(svc =>
                {
                    svc.AddDbContextPool<CrmsContext>(options =>
                        options.UseMySQL("Server=localhost;Database=crmsdb;Uid=root;Pwd=root;SslMode=none")
                    );
                    svc
                        .AddViceVersaClassDao()
                        .AddViceVersaClassService()
                        .AddViceVersaCourseDao()
                        .AddViceVersaCourseService()
                        .AddViceVersaGradeDao()
                        .AddViceVersaGradeService()
                        .AddHighGradeSchoolService()
                        .AddHighGradeSeminarService()
                        .AddInsomniaFixedGroupService()
                        .AddInsomniaLoginService()
                        .AddInsomniaSeminarGroupService()
                        .AddInsomniaTopicService()
                        .AddInsomniaUserService();
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(HighGradeExtensions).Assembly).WithReferences())
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(ViceVersaExtensions).Assembly).WithReferences())
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(InsomniaExtensions).Assembly).WithReferences())
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

        public static IPAddress GetLocalIpAddress()
        {
            UnicastIPAddressInformation mostSuitableIp = null;

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var network in networkInterfaces)
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                var properties = network.GetIPProperties();

                if (properties.GatewayAddresses.Count == 0)
                {
                    continue;
                }

                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    if (IPAddress.IsLoopback(address.Address))
                    {
                        continue;
                    }

                    if (!address.IsDnsEligible)
                    {
                        if (mostSuitableIp == null)
                        {
                            mostSuitableIp = address;
                        }

                        continue;
                    }

                    // The best IP is the IP got from DHCP server
                    if (address.PrefixOrigin == PrefixOrigin.Dhcp)
                    {
                        return address.Address;
                    }

                    if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                    {
                        mostSuitableIp = address;
                    }
                }
            }

            return mostSuitableIp?.Address;
        }
    }
}
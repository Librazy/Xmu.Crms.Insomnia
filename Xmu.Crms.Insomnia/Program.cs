using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xmu.Crms.Services.Insomnia;
using Xmu.Crms.Shared;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Insomnia
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var conf = services.GetRequiredService<IConfiguration>();
                var db = services.GetService<CrmsContext>();

                if (Convert.ToBoolean(conf["Database:EnsureCreated"]))
                {
                    await db.Database.EnsureCreatedAsync();
                }

                if (Convert.ToBoolean(conf["Database:Migrate"]))
                {
                    await db.Database.MigrateAsync();
                }
            }

            host.Run();
        }


        public static IWebHost BuildWebHost(string[] args) =>
            CreateWebHostBuilder(args)
                .Build();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .ConfigureServices(collection =>
                {
                    collection
                        .AddInsomniaSeminarGroupService()
                        .AddInsomniaFixedGroupService()
                        .AddInsomniaLoginService()
                        .AddInsomniaTopicService()
                        .AddInsomniaUserService()
                        .AddCrmsView("API.Insomnia")
                        .AddCrmsView("Web.Insomnia")

                        .AddViceVersaClassDao()
                        .AddViceVersaClassService()
                        .AddViceVersaCourseDao()
                        .AddViceVersaCourseService()
                        .AddViceVersaGradeDao()
                        .AddViceVersaGradeService()

                        .AddHighGradeSchoolService()
                        .AddHighGradeSeminarService()
                        ;
                })
                .UseStartup<Startup>();
        }
    }
}
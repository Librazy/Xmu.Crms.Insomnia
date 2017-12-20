﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

                if (Convert.ToBoolean(conf["Database:InsertStub"]))
                {
                    var school = await db.School.AddAsync(new School
                    {
                        City = "厦门",
                        Name = "厦门市人民公园",
                        Province = "福建"
                    });

                    await db.SaveChangesAsync();

                    await db.UserInfo.AddAsync(new UserInfo
                    {
                        Avatar = "/upload/avatar/Logo_Li.png",
                        Email = "t@t.test",
                        Gender = 0,
                        Name = "张三",
                        Number = "123456",
                        Password = PasswordUtils.HashString("123"),
                        Phone = "1234",
                        School = await db.School.FindAsync(school.Entity.Id),
                        Title = 1
                    });

                    await db.UserInfo.AddAsync(new UserInfo
                    {
                        Avatar = "/upload/avatar/Logo_Li.png",
                        Email = "t2@t.test",
                        Gender = 1,
                        Name = "李四",
                        Number = "134254",
                        Password = PasswordUtils.HashString("456"),
                        Phone = "123",
                        School = await db.School.FindAsync(school.Entity.Id),
                        Title = 1
                    });

                    await db.SaveChangesAsync();
                }
            }

            host.Run();
        }


        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .ConfigureServices(collection =>
                {
                    collection.AddInsomniaUserService().AddInsomniaTimerService().AddCrmsView("Web.Insomnia");
                })
                .UseStartup<Startup>()
                .Build();
    }
}
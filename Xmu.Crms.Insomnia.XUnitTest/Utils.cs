using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.PlatformAbstractions;

namespace Xmu.Crms.Insomnia.XUnitTest
{
    internal static class Utils
    {
        public static async Task<SqliteConnection> PopulateDbAsync(this SqliteConnection connection, string basePath,
            string fileName = "init.sqlite")
        {
            var transaction = connection.BeginTransaction();
            var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = new StreamReader(
                new PhysicalFileProvider(basePath).GetFileInfo(fileName).CreateReadStream()
            ).ReadToEnd();
            await cmd.ExecuteNonQueryAsync();
            cmd.Transaction.Commit();
            return connection;
        }

        public static TestServer MakeTestServer(this SqliteConnection connection, string basePath)
        {
            var webHostBuilder = Program.CreateWebHostBuilder(new string[0]);
            webHostBuilder
                .UseContentRoot(basePath)
                .ConfigureAppConfiguration(config => config.AddJsonFile(basePath + "\\appsettings.json", true))
                .ConfigureAppConfiguration(config => config.AddJsonFile(basePath + "\\appsettings.XUnit.json"))
                .ConfigureServices(service => service.UseCrmsSqlite(connection))
                .UseEnvironment("Development");
            var server = new TestServer(webHostBuilder);
            return server;
        }

        public static string GetProjectPath(Assembly startupAssembly)
        {
            //Get name of the target project which we want to test
            var projectName = startupAssembly.GetName().Name;

            //Get currently executing test project path
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;

            //Find the folder which contains the solution file. We then use this information to find the 
            //target project which we want to test
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                if (directoryInfo.Name == projectName)
                {
                    return directoryInfo.FullName;
                }

                directoryInfo = directoryInfo.Parent;
            } while (directoryInfo?.Parent != null);

            throw new Exception($"Solution root could not be located using application root {applicationBasePath}");
        }
    }
}
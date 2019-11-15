using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;

using Serilog;

namespace Pnnl.Api.Approvals.Host
{
    public class Program
    {
        /// <summary>
        /// Main entry point for the executable.
        /// </summary>
        /// <param name="args">The command line arguments provided at runtime.</param>
        public static void Main(string[] args)
        {
            GetWebHostBuilder(args)
                .Build()
                .Run();
        }

        /// <summary>
        /// Gets the builder that describes the the web host.
        /// </summary>
        /// <param name="args">The optional command line arguments provided at runtime.</param>
        /// <returns>The <see cref="IWebHostBuilder"/> hat describes the the web host.</returns>
        public static IWebHostBuilder GetWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)

                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(context.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true,
                            reloadOnChange: true)
                        .AddEnvironmentVariables();

                    var builtConfig = builder.Build();
                    var env = context.HostingEnvironment;
                    // Use staging config for QA environment
                    var configEnv = env.EnvironmentName.ToLower() == "qa" ? "staging" : env.EnvironmentName;

                })

                .ConfigureLogging((context, builder) =>
                {
                    builder
                        .AddSerilog(
                        new LoggerConfiguration()
                            .ReadFrom.Configuration(context.Configuration)
                            .CreateLogger()
                    );
                })

                //.UseApplicationInsights()

                .UseStartup<Startup>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace GlobalSolutions
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();
            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                 .Enrich.FromLogContext()
                 .WriteTo.File("logs\\logs.txt",
                     fileSizeLimitBytes: 1_000_000,
                     rollOnFileSizeLimit: true,
                     shared: true,
                     flushToDiskInterval: TimeSpan.FromSeconds(1),
                     restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
                 .CreateLogger();

            try
            {
                BuildWebHost(args).Run();
            }
            catch (Exception)
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
    }
}

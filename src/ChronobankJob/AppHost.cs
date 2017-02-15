using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using AzureRepositories;
using Core.Settings;
using LkeServices.Triggers;
using Microsoft.Extensions.Configuration;

namespace ChronobankJob
{
    public class AppHost
    {
        public IConfigurationRoot Configuration { get; }

#if DEBUG
        const string SettingsBlobName = "chronobanksettings.json";
#else
        const string SettingsBlobName = "globalsettings.json";
#endif

        public AppHost()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void Run()
        {
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(Configuration.GetConnectionString("Azure"), SettingsBlobName);

            var containerBuilder = new AzureBinder().Bind(settings);
            var ioc = containerBuilder.Build();

            var triggerHost = new TriggerHost(new AutofacServiceProvider(ioc));

            triggerHost.ProvideAssembly(GetType().GetTypeInfo().Assembly);

            var end = new ManualResetEvent(false);

            AssemblyLoadContext.Default.Unloading += ctx =>
            {
                Console.WriteLine("SIGTERM recieved");
                triggerHost.Cancel();

                end.WaitOne();
            };

            triggerHost.StartAndBlock();
            end.Set();
        }
    }
}

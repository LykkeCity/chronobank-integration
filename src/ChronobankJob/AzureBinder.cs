using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.ResolveAnything;
using AzureStorage.Tables;
using Common.Log;
using Core.Settings;
using AzureRepositories;
using AzureRepositories.Log;
using Common;
using LkeServices;
using Lykke.JobTriggers.Extenstions;
using Microsoft.Extensions.DependencyInjection;

namespace ChronobankJob
{
    public class AzureBinder
    {
        public const string DefaultConnectionString = "UseDevelopmentStorage=true";

        public ContainerBuilder Bind(BaseSettings settings)
        {
            var logToTable = new LogToTable(new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "LogChronobankJobError", null),
                                            new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "LogChronobankJobWarning", null),
                                            new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "LogChronobankJobInfo", null));
            var log = new LogToTableAndConsole(logToTable, new LogToConsole());
            var ioc = new ContainerBuilder();
            InitContainer(ioc, settings, log);
            return ioc;
        }

        private void InitContainer(ContainerBuilder ioc, BaseSettings settings, ILog log)
        {
#if DEBUG
            log.WriteInfoAsync("Chronobank Job", "App start", null, $"BaseSettings : {settings.ToJson()}").Wait();
#else
            log.WriteInfoAsync("Chronobank Job", "App start", null, $"BaseSettings : private").Wait();
#endif
            ioc.RegisterInstance(log);
            ioc.RegisterInstance(settings);

            ioc.BindCommonServices();
            ioc.BindAzure(settings, log);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(log);

            serviceCollection.AddTriggers(pool =>
            {
                pool.AddDefaultConnection(settings.Db.DataConnString);
                pool.AddConnection("cashout", settings.Db.ChronoNotificationConnString);
            });
            ioc.Populate(serviceCollection);

            ioc.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
        }
    }
}

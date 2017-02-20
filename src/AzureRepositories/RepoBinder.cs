using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AzureRepositories.ApiRequests;
using AzureRepositories.Monitoring;
using AzureRepositories.Notifiers;
using AzureRepositories.TransactionMonitoring;
using AzureRepositories.UserContracts;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Core;
using Core.IssueNotifier;
using Core.Notifiers;
using Core.Repositories.ApiRequests;
using Core.Repositories.Monitoring;
using Core.Repositories.UserContracts;
using Core.Settings;
using Core.TransactionMonitoring;

namespace AzureRepositories
{
    public static class RepoBinder
    {
        public static void BindAzure(this ContainerBuilder ioc, BaseSettings settings, ILog log)
        {
            ioc.BindRepo(settings, log);
            ioc.BindQueue(settings);

            ioc.RegisterType<EmailNotifier>().As<IEmailNotifier>();
            ioc.RegisterType<SlackNotifier>().As<ISlackNotifier>();
        }

        private static void BindRepo(this ContainerBuilder ioc, BaseSettings settings, ILog log)
        {
            ioc.RegisterInstance(new ApiRequestBlobRepository(new AzureBlobStorage(settings.Db.LogsConnString)))
                .As<IApiRequestBlobRepository>();

            ioc.RegisterInstance(new MonitoringRepository(new AzureTableStorage<MonitoringEntity>(settings.Db.SharedConnString, "Monitoring", log)))
               .As<IMonitoringRepository>();

            ioc.RegisterInstance(new UserContractRepository(new AzureTableStorage<UserContractEntity>(settings.Db.DataConnString, "UserContracts", log)))
               .As<IUserContractRepository>();
        }

        private static void BindQueue(this ContainerBuilder ioc, BaseSettings settings)
        {
            ioc.RegisterType<TransactionMonitoringQueueWriter>().As<ITransactionMonitoringQueueWriter>().SingleInstance();
            ioc.RegisterType<IssueNotifier.IssueNotifier>().As<IIssueNotifier>().SingleInstance();

            ioc.RegisterInstance<Func<string, IQueueExt>>(queueName =>
            {
                switch (queueName)
                {
                    case Constants.SlackNotifierQueue:
                    case Constants.EmailNotifierQueue:
                        return new AzureQueueExt(settings.Db.SharedConnString, queueName);
                    case Constants.IssueNotifyQueue:
                        return new AzureQueueExt(settings.Db.ChronoNotificationConnString, queueName);
                    default:
                        return new AzureQueueExt(settings.Db.DataConnString, queueName);
                }

            });
        }
    }
}

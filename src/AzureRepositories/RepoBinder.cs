using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AzureRepositories.Notifiers;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Core;
using Core.Notifiers;
using Core.Settings;

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

        }

        private static void BindQueue(this ContainerBuilder ioc, BaseSettings settings)
        {

            ioc.RegisterInstance<Func<string, IQueueExt>>(queueName =>
            {
                switch (queueName)
                {
                    case Constants.SlackNotifierQueue:
                    case Constants.EmailNotifierQueue:
                        return new AzureQueueExt(settings.Db.SharedConnString, queueName);
                    default:
                        return new AzureQueueExt(settings.Db.DataConnString, queueName);
                }

            });
        }
    }
}

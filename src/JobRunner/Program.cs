using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronobankJob;
using Core.TransactionMonitoring;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace JobRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Clear();
            Console.Title = "Bitcoin job - Ver. " + Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion;

            var queue = new CloudQueue(new Uri(
                "https://lkechronobank.queue.core.windows.net/transaction-monitoring-poison?st=2017-12-14T02%3A24%3A00Z&se=2017-12-15T02%3A24%3A00Z&sp=r&sv=2017-04-17&sig=7ll7hV%2BjGSdUXo1J3%2FIOG1vEutMzrnI8EhAYNR1II%2BU%3D"));
            
            var normalQueue = new CloudQueue(new Uri(
                "https://lkechronobank.queue.core.windows.net/transaction-monitoring?st=2017-12-14T07%3A24%3A00Z&se=2017-12-15T07%3A24%3A00Z&sp=ra&sv=2017-04-17&sig=6ePwZtMxT556KavBsm1bhm66xU8EyB4nMKijbClgyDE%3D"));

            while (true)
            {
                var msg = queue.GetMessageAsync().Result;

                if (msg == null)
                    break;

                var data = JsonConvert.DeserializeObject<TransactionMonitoringMessage>(msg.AsString);

                if (data.Type == TransactionType.Cashout)
                {
                    continue;
                }

                normalQueue.AddMessageAsync(msg).Wait();
            }

            var host = new AppHost();

            Console.WriteLine($"Bitcoin job is running");
            Console.WriteLine("Utc time: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            host.Run();
        }
    }
}

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

            var host = new AppHost();

            Console.WriteLine($"Bitcoin job is running");
            Console.WriteLine("Utc time: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            host.Run();
        }
    }
}

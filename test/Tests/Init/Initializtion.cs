using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureRepositories;
using Common.Log;
using Core.Settings;
using NUnit.Framework;
using Common;
using LkeServices;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Tests
{
    [SetUpFixture]
    public class Config
    {
        public static IServiceProvider Services { get; set; }
        public static ILog Logger => Services.GetService<ILog>();

        private Settings ReadSettings()
        {
            try
            {
                var json = File.ReadAllText(@"..\..\settings\buildersettings.json");
                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }
                Settings settings = json.DeserializeJson<Settings>();

                return settings;
            }
            catch (Exception)
            {
                return null;
            }
        }


        [OneTimeSetUp]
        public void Initialize()
        {
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>("UseDevelopmentStorage=true", "chronobanksettings.json");

            var log = new LogToConsole();

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(settings);
            builder.RegisterInstance(log).As<ILog>();
            builder.BindAzure(settings, log);

            builder.BindCommonServices();

            var testSettings = ReadSettings();
            if (testSettings != null)
                builder.RegisterInstance(testSettings);

            Services = new AutofacServiceProvider(builder.Build());
        }
    }

    public class Settings
    {
        public Dictionary<ContractType, string> Addresses { get; set; }
    }

    public enum ContractType
    {
        Platform,
        Asset,
        AssetProxy,
        Emmiter,
        EventHistory
    }
}

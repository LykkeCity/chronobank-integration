﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureRepositories;
using Common.Log;
using Core.Settings;
using Common;
using Core;
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


        [Fact]
        public void Initialize()
        {
            var generalSettings = GeneralSettingsReader.ReadGeneralSettingsLocal<BaseSettings>("../../settings/chronobanksettings.json");
            
            var log = new LogToConsole();

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(generalSettings);
            builder.RegisterInstance(log).As<ILog>();
            builder.BindAzure(generalSettings, log);

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

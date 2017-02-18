using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Core.Providers;
using Core.Settings;
using LkeServices.Rest;
using Nethereum.Web3;
using RestEase;

namespace LkeServices
{
    public static class SrvBinder
    {
        public static void BindCommonServices(this ContainerBuilder ioc)
        {
            ioc.Register(x => new Web3(x.Resolve<BaseSettings>().EthereumUrl));
        }


        public static void BindApiProviders(this ContainerBuilder ioc)
        {
            ioc.Register(x =>
            {
                var resolver = x.Resolve<IComponentContext>();
                var lykkyHttpClientHandler = resolver.Resolve<LykkeHttpClientHandler>();
                var settings = resolver.Resolve<BaseSettings>();
                var client = new HttpClient(lykkyHttpClientHandler)
                {
                    BaseAddress = new Uri(settings.SignatureProviderUrl)
                };
                return RestClient.For<ISignatureApi>(client);
            }).As<ISignatureApi>().SingleInstance();
        }
    }
}

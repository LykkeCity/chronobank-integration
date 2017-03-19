using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Core.Contracts;
using Core.Ethereum;
using Core.Providers;
using Core.Settings;
using LkeServices.Contracts;
using LkeServices.Ethereum;
using LkeServices.Rest;
using LkeServices.Signature;
using Microsoft.Extensions.Configuration;
using Nethereum.Geth;
using Nethereum.Web3;
using RestEase;

namespace LkeServices
{
    public static class SrvBinder
    {
        public static void BindCommonServices(this ContainerBuilder ioc)
        {
            ioc.Register(x =>
            {
                var resolver = x.Resolve<IComponentContext>();
                var web3 = new Web3(resolver.Resolve<BaseSettings>().EthereumUrl);
                web3.Client.OverridingRequestInterceptor = new SignatureInterceptor(resolver.Resolve<ISignatureApi>(), web3);

                return web3;
            });

            ioc.Register(x =>
            {
                var resolver = x.Resolve<IComponentContext>();
                var web3 = new Web3Geth(resolver.Resolve<BaseSettings>().EthereumUrl);
                return web3;
            });

            ioc.RegisterType<ContractService>().As<IContractService>();
            ioc.RegisterType<PaymentService>().As<IPaymentService>();
            ioc.RegisterType<TransactionService>().As<ITransactionService>();
            ioc.RegisterType<UserContractQueueService>().As<IUserContractQueueService>().SingleInstance();

            ioc.BindApiProviders();
        }


        private static void BindApiProviders(this ContainerBuilder ioc)
        {
            ioc.RegisterType<LykkeHttpClientHandler>().SingleInstance();

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

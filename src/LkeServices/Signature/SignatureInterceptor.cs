using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Providers;
using Core.Settings;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;

namespace LkeServices.Signature
{
    public class SignatureInterceptor : RequestInterceptor
    {
        private readonly ITransactionManager _transactionManager;

        public SignatureInterceptor(ISignatureApi signatureApi, Web3 web3, BaseSettings baseSettings)
        {
            _transactionManager = new LykkeSignedTransactionManager(web3, signatureApi, baseSettings);
        }

        public override async Task<object> InterceptSendRequestAsync<TResponse>(
            Func<RpcRequest, string, Task<TResponse>> interceptedSendRequestAsync, RpcRequest request,
            string route = null)
        {
            if (request.Method == "eth_sendTransaction")
            {
                var transaction = (TransactionInput)request.RawParameters[0];
                return await SignAndSendTransaction(transaction).ConfigureAwait(false);
            }
            return await base.InterceptSendRequestAsync(interceptedSendRequestAsync, request, route).ConfigureAwait(false);
        }

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<string, string, object[], Task<T>> interceptedSendRequestAsync, string method,
            string route = null, params object[] paramList)
        {
            if (method == "eth_sendTransaction")
            {
                var transaction = (TransactionInput)paramList[0];
                return await SignAndSendTransaction(transaction).ConfigureAwait(false);
            }
            return await base.InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList).ConfigureAwait(false);
        }

        private async Task<string> SignAndSendTransaction(TransactionInput transaction)
        {
            return await _transactionManager.SendTransactionAsync(transaction).ConfigureAwait(false);
        }
    }
}

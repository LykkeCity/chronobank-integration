using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Providers;
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

        public SignatureInterceptor(ISignatureApi signatureApi, Web3 web3)
        {
            _transactionManager = new LykkeSignedTransactionManager(web3, signatureApi);
        }

        public override Task InterceptSendRequestAsync(Func<RpcRequest, string, Task> interceptedSendRequestAsync, RpcRequest request, string route = null)
        {
            if (request.Method == "eth_sendTransaction")
            {
                TransactionInput transaction = (TransactionInput)request.RawParameters[0];
                return SignAndSendTransaction(transaction, route);
            }
            return base.InterceptSendRequestAsync(interceptedSendRequestAsync, request, route);
        }

        public override Task InterceptSendRequestAsync(Func<string, string, object[], Task> interceptedSendRequestAsync, string method, string route = null, params object[] paramList)
        {
            if (method == "eth_sendTransaction")
            {
                TransactionInput transaction = (TransactionInput)paramList[0];
                return SignAndSendTransaction(transaction, route);
            }
            return base.InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList);
        }

        //public RpcResponse BuildResponse(object results, string route = null)
        //{
        //    return new RpcResponse(route, JToken.FromObject(results));
        //}

        //public override async Task<RpcResponse> InterceptSendRequestAsync(Func<RpcRequest, string, Task<RpcResponse>> interceptedSendRequestAsync, RpcRequest request, string route = null)
        //{
        //    if (request.Method == "eth_sendTransaction")
        //    {
        //        TransactionInput transaction = (TransactionInput)request.ParameterList[0];
        //        return await SignAndSendTransaction(transaction, route);
        //    }
        //    return await interceptedSendRequestAsync(request, route).ConfigureAwait(false);
        //}

        //public override async Task<RpcResponse> InterceptSendRequestAsync(Func<string, string, object[], Task<RpcResponse>> interceptedSendRequestAsync, string method, string route = null, params object[] paramList)
        //{
        //    if (method == "eth_sendTransaction")
        //    {
        //        TransactionInput transaction = (TransactionInput)paramList[0];
        //        return await SignAndSendTransaction(transaction, route);
        //    }
        //    return await interceptedSendRequestAsync(method, route, paramList).ConfigureAwait(false);
        //}

        private Task SignAndSendTransaction(TransactionInput transaction, string route)
        {
            return _transactionManager.SendTransactionAsync(transaction);
        }
    }
}

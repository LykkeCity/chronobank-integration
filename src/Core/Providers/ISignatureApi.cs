using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using RestEase;

namespace Core.Providers
{
    public interface ISignatureApi
    {
        [Post("api/ethereum/sign")]
        Task<string> SignTransaction([Body] SignRequest request);
    }

    public class SignRequest
    {
        public string From { get; set; }

        public string Transaction { get; set; }
    }
}

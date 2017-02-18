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

        public string To { get; set; }

        public string Data { get; set; }

        public string GasHex { get; set; }

        public string GasPriceHex { get; set; }

        public string ValueHex { get; set; }

        public string NonceHex { get; set; }

        public SignRequest(TransactionInput transaction)
        {
            From = transaction.From;
            To = transaction.To;
            Data = transaction.Data;
            GasHex = transaction.Gas.HexValue;
            GasPriceHex = transaction.GasPrice.HexValue;
            ValueHex = transaction.Value.HexValue;
            NonceHex = transaction.Nonce.HexValue;
        }
    }
}

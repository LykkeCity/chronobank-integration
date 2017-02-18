using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Ethereum;
using Core.Settings;
using Nethereum.Util;
using Nethereum.Web3;

namespace LkeServices.Ethereum
{
    public class PaymentService : IPaymentService
    {
        private readonly Web3 _web3;
        private readonly BaseSettings _settings;

        public PaymentService(Web3 web3, BaseSettings settings)
        {
            _web3 = web3;
            _settings = settings;
        }

        public async Task<decimal> GetMainAccountBalance()
        {
            var balance = await _web3.Eth.GetBalance.SendRequestAsync(_settings.EthereumMainAccount);
            return UnitConversion.Convert.FromWei(balance);
        }
    }
}

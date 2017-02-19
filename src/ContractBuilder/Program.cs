using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LkeServices.Contracts;
using Nethereum.ABI.JsonDeserialisation;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace ContractBuilder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ChronobankBuilder("0x9c834B1a7c18F902C376907B15846EF7B1420286", "123456");
            builder.Build().Wait();
            Console.ReadLine();
        }
    }
}

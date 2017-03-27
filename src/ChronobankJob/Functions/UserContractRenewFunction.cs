using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Core.Contracts;
using Lykke.JobTriggers.Triggers.Attributes;

namespace ChronobankJob.Functions
{
    public class UserContractsRenewFunction
    {
        private readonly IUserContractQueueService _contractQueueService;
        private readonly ILog _logger;

        public UserContractsRenewFunction(IUserContractQueueService contractQueueService, ILog logger)
        {
            _contractQueueService = contractQueueService;
            _logger = logger;
        }

        [TimerTrigger("23:00:00")]
        public async Task Renew()
        {
            await _logger.WriteInfoAsync(nameof(UserContractsRenewFunction), nameof(Renew), "", "Start user contracts renewing");

            var count = await _contractQueueService.Count();
            for (var i = 0; i < count; i++)
            {
                var contract = await _contractQueueService.GetContractRaw();
                if (string.IsNullOrWhiteSpace(contract))
                    return;
                await _contractQueueService.PushContract(contract);
            }

            await _logger.WriteInfoAsync(nameof(UserContractsRenewFunction), nameof(Renew), "", "Finish user contracts renewing");
        }
    }
}

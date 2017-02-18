using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Repositories.Monitoring;
using LkeServices.Triggers.Attributes;

namespace ChronobankJob.Functions
{
    public class MonitoringFunction
    {
        private readonly IMonitoringRepository _repository;

        public MonitoringFunction(IMonitoringRepository repository)
        {
            _repository = repository;
        }

        [TimerTrigger("00:00:30")]
        public async Task Execute()
        {
            await _repository.SaveAsync(new Monitoring
            {
                DateTime = DateTime.UtcNow,
                ServiceName = "ChronobankService",
                Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion
            });
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SentinelDoCloudinho.Services;

namespace SentinelDoCloudinho.Services.Hubs
{
    public class TestHub : Hub
    {
        private readonly SecurityTestService _service;

        public TestHub(SecurityTestService service)
        {
            _service = service;
        }

        public async Task StartPenTest(List<string> targets, string aggressionLevel)
        {
            await _service.PerformPenTest(targets, aggressionLevel);
        }
    }
}

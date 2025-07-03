using Microsoft.AspNetCore.SignalR;

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
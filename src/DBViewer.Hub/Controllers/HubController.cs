using Dawn;
using DbViewer.Hub.Services;
using DbViewer.Shared.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DbViewer.Hub.Controllers
{
    [ApiController]
    [Route("Hubs")]
    public class HubController : ControllerBase
    {
        private readonly ILogger<HubController> _logger;
        private readonly IHubService _hubService;

        public HubController(ILogger<HubController> logger, IHubService hubService)
        {
            _hubService = Guard.Argument(hubService, nameof(hubService))
                  .NotNull()
                  .Value;

            _logger = Guard.Argument(logger, nameof(logger))
                  .NotNull()
                  .Value;
        }

        [HttpGet]
        public HubInfo GetHubInfo()
        {
            _logger.LogInformation("Fetching Hub Info");

            var hubInfo = _hubService.GetLatestHub();

            return hubInfo;
        }

        [HttpPut("Update/{hubInfo}")]
        public void Update(HubInfo hubInfo)
        {
            _hubService.SaveLatestHub(hubInfo);
        }
    }
}

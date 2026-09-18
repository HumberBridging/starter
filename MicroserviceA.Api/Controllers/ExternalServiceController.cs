using MicroserviceA.Api.Clients;
using Microsoft.AspNetCore.Mvc;

namespace MicroserviceA.Api.Controllers
{
    [Route("api/external-service")]
    [ApiController]
    public class ExternalServiceController : ControllerBase
    {
        private readonly MicroserviceBClient _microserviceB;
        private readonly ILogger<ExternalServiceController> _logger;

        public ExternalServiceController(MicroserviceBClient microserviceB, ILogger<ExternalServiceController> logger)
        {
            _microserviceB = microserviceB;
            _logger = logger;
        }

        // GET api/external-service?mode=random
        [HttpGet]
        public async Task<ActionResult> GetExternalData([FromQuery] string? mode, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calling MicroserviceB unstable endpoint (mode {Mode})", mode ?? "random");

            using var response = await _microserviceB.GetUnstableAsync(mode, cancellationToken);

            return await BuildRelayResultAsync(response, cancellationToken);
        }

        // GET api/external-service/slow?ms=3000
        [HttpGet("slow")]
        public async Task<ActionResult> GetSlowData([FromQuery] int ms, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Calling MicroserviceB slow endpoint ({DelayMs} ms)", ms);

            using var response = await _microserviceB.GetSlowAsync(ms, cancellationToken);
            
            return await BuildRelayResultAsync(response, cancellationToken);
        }

        // POST api/external-service/orders?mode=random
        [HttpPost("orders")]
        public async Task<ActionResult> CreateOrder([FromQuery] string? mode, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating an order in MicroserviceB (mode {Mode})", mode ?? "random");

            using var response = await _microserviceB.CreateOrderAsync(mode, cancellationToken);

            return await BuildRelayResultAsync(response, cancellationToken);
        }

        //Helpers
        private async Task<ActionResult> BuildRelayResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Final status code: {StatusCode}", (int)response.StatusCode);

            return StatusCode((int)response.StatusCode, new
            {
                finalStatusCode = (int)response.StatusCode,
                response = content
            });
        }
    }
}

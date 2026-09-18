using System.ComponentModel.DataAnnotations;
using MicroserviceB.Api.Chaos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MicroserviceB.Api.Controllers
{
    /// <summary>
    /// A deliberately unreliable service. Use ?mode= to force an outcome:
    /// random (default) · ok · notfound · error · unavailable · throw
    /// </summary>
    [Route("api/demo")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly ILogger<DemoController> _logger;
        private readonly ChaosOptions _chaos;

        public DemoController(ILogger<DemoController> logger, IOptions<ChaosOptions> chaos)
        {
            _logger = logger;
            _chaos = chaos.Value;
        }

        // GET api/demo/unstable?mode=random
        [HttpGet("unstable")]
        public IActionResult GetUnstableEndpoint([FromQuery] string mode = "random")
        {
            _logger.LogInformation("Unstable endpoint called with mode {Mode}", mode);

            return Simulate(mode, () => Ok(new
            {
                success = true,
                message = "Request succeeded",
                timestamp = DateTime.UtcNow
            }));
        }

        // POST api/demo/orders?mode=random — a NON-idempotent call: retrying it could create two orders
        [HttpPost("orders")]
        public IActionResult CreateOrder([FromQuery] string mode = "random")
        {
            _logger.LogInformation("Create order called with mode {Mode}", mode);

            return Simulate(mode, () => StatusCode(StatusCodes.Status201Created, new
            {
                orderId = Guid.NewGuid(),
                timestamp = DateTime.UtcNow
            }));
        }

        // GET api/demo/slow?ms=3000 — answers eventually, for timeout demos
        [HttpGet("slow")]
        public async Task<IActionResult> GetSlowEndpoint([FromQuery, Range(0, 30_000)] int ms = 3000, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Slow endpoint called, delaying {DelayMs} ms", ms);

            await Task.Delay(ms, cancellationToken);

            return Ok(new 
            { success = true, delayedMs = ms });
        }

        private IActionResult Simulate(string mode, Func<IActionResult> onSuccess)
        {
            var outcome = mode.ToLowerInvariant() switch
            {
                "random" => PickRandomOutcome(),
                "ok" or "notfound" or "error" or "unavailable" or "throw" => mode.ToLowerInvariant(),
                _ => null
            };

            if (outcome is null)
            {
                return BadRequest($"Unknown mode '{mode}'. Use random, ok, notfound, error, unavailable or throw.");
            }

            if (outcome == "ok")
            {
                return onSuccess();
            }

            // Everything below is a simulated failure
            _logger.LogWarning("Simulated failure occurred: {Outcome}", outcome);

            return outcome switch
            {
                "notfound" => NotFound("Simulated 404 Not Found"),
                "error" => StatusCode(StatusCodes.Status500InternalServerError, "Simulated 500 Internal Server Error"),
                "unavailable" => StatusCode(StatusCodes.Status503ServiceUnavailable, "Simulated 503 Service Unavailable"),
                // Mimic an unhandled exception to trigger the global exception handler
                _ => throw new NotImplementedException("Simulated unhandled exception")
            };
        }

        // Of the failures: 1 in 5 is a 404, 2 in 5 throw, 1 in 5 is a 500, 1 in 5 is a 503
        private string PickRandomOutcome()
        {
            if (Random.Shared.NextDouble() < _chaos.SuccessRate)
            {
                return "ok";
            }

            return Random.Shared.Next(1, 6) switch
            {
                1 => "notfound",
                2 or 3 => "throw",
                4 => "error",
                _ => "unavailable"
            };
        }
    }
}

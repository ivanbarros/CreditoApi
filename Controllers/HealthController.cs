using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CreditoAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        /// <summary>
        /// Verifica a saúde básica do serviço
        /// </summary>
        /// <returns>Status do serviço</returns>
        [HttpGet("self")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Self()
        {
            _logger.LogInformation("Self health check called");
            
            return Ok(new
            {
                status = "healthy",
                service = "CreditoAPI",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Verifica se o serviço está pronto para receber requisições
        /// </summary>
        /// <returns>Status de prontidão do serviço</returns>
        [HttpGet("ready")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Ready()
        {
            _logger.LogInformation("Ready health check called");

            var healthReport = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = healthReport.Status.ToString(),
                checks = healthReport.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = healthReport.TotalDuration.TotalMilliseconds,
                timestamp = DateTime.UtcNow
            };

            if (healthReport.Status == HealthStatus.Healthy)
            {
                return Ok(response);
            }

            return StatusCode(503, response);
        }
    }
}

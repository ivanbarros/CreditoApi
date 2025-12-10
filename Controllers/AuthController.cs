using CreditoAPI.DTOs;
using CreditoAPI.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace CreditoAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IJwtTokenService jwtTokenService, ILogger<AuthController> logger)
        {
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Username == "admin" && request.Password == "admin123")
            {
                var token = _jwtTokenService.GenerateToken("1", request.Username, new[] { "Admin" });
                
                return Ok(new LoginResponse
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    Username = request.Username
                });
            }

            if (request.Username == "user" && request.Password == "user123")
            {
                var token = _jwtTokenService.GenerateToken("2", request.Username, new[] { "User" });
                
                return Ok(new LoginResponse
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    Username = request.Username
                });
            }

            _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
            return Unauthorized(new { error = "Invalid credentials" });
        }
    }
}

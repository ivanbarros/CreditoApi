namespace CreditoAPI.Infrastructure.Authentication
{
    public interface IJwtTokenService
    {
        string GenerateToken(string userId, string username, IEnumerable<string> roles);
        bool ValidateToken(string token);
    }
}

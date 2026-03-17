namespace Rafiq.Application.Interfaces.External;

public interface IJwtTokenService
{
    string GenerateAccessToken(int userId, string email, IReadOnlyCollection<string> roles, string securityStamp);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiryUtc();
}

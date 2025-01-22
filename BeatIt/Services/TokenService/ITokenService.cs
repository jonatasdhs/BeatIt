using System.Security.Claims;
using BeatIt.Models;

namespace BeatIt.Services.TokenService;

public interface ITokenService {
    public string GenerateJwtToken(User user);
    public string GenerateRefreshToken();
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    public Guid GetUserIdFromToken(string token);
}
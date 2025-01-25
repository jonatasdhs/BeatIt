using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeatIt.Services.TokenService;
using BeatIt.Tests.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace BeatIt.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<SymmetricSecurityKey> _secretKeyMock;
    private readonly TokenService _tokenService;
    private readonly Mock<ILogger<TokenService>> _logger;
    private readonly Mock<IConfiguration> _config;
    private readonly ITestOutputHelper _output;
    public TokenServiceTests(ITestOutputHelper output)
    {
        _secretKeyMock = new Mock<SymmetricSecurityKey>();
        _logger = new Mock<ILogger<TokenService>>();
        _config = new Mock<IConfiguration>();
        _output = output;
        _config
            .Setup(config => config["JwtSettings:Secret"])
            .Returns("secretasdhaiwuheiuahsdiuansdiunqwieasdzxc");

        _tokenService = new TokenService(_config.Object, _logger.Object);
    }

    [Fact]
    public void GenerateJwtToken_ReturnValidToken()
    {
        // Arrange
        var user = TestData.GetValidUser();

        // Act
        var result = _tokenService.GenerateJwtToken(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadToken(result) as JwtSecurityToken;

        var claims = token?.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
        Assert.NotNull(claims);
        foreach (var claim in claims)
        {
            _output.WriteLine(claim);
        }
        // Assert
        Assert.NotNull(token);
        Assert.Equal(user.Id.ToString(), token.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value);
        Assert.Equal(user.Name, token.Claims.First(c => c.Type == "unique_name").Value);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueToken()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token1);
        Assert.NotNull(token2);
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var token = _tokenService.GenerateJwtToken(user);

        // Act
        var principal = _tokenService.GetPrincipalFromExpiredToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(user.Name, principal.FindFirst(ClaimTypes.Name)?.Value);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldThrowForInvalidToken()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenService.GetPrincipalFromExpiredToken(invalidToken));
    }

    [Fact]
    public void GetUserIdFromToken_ShouldReturnUserId()
    {
        // Arrange
        var user = TestData.GetValidUser();
        var token = _tokenService.GenerateJwtToken(user);

        // Act
        var userId = _tokenService.GetUserIdFromToken(token);

        // Assert
        Assert.Equal(user.Id, userId);
    }

    [Fact]
    public void GetUserIdFromToken_ShouldThrowForInvalidToken()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenService.GetUserIdFromToken(invalidToken));
    }

    [Fact]
    public void GetUserIdFromToken_ShouldThrowIfNameIdClaimIsMissing()
    {
        // Arrange
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test User") }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("supersecretkeyforsigningjwt12345")),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => _tokenService.GetUserIdFromToken(token));
    }

}
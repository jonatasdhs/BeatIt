using Moq;
using BeatIt.Services.AuthService;
using BeatIt.Repositories;
using BeatIt.Models.DTOs;
using BeatIt.Services.TokenService;
using BeatIt.Services.PasswordService;
using Microsoft.Extensions.Logging;
using BeatIt.Models;
using Xunit;
using BeatIt.Errors;
using System.Security.Claims;
using BeatIt.Services.CacheService;

namespace BeatIt.Tests.Services;
public class AuthServicesTests
{

    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<IPasswordService> _hasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService authService;

    public AuthServicesTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _cacheMock = new Mock<ICacheService>();
        _hasherMock = new Mock<IPasswordService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        authService = new AuthService(
            _userRepositoryMock.Object,
            _cacheMock.Object,
            _hasherMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object
        );
    }
    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Password = "hashedPassword",
            Salt = "salt"
        };

        var loginDto = new UserLoginDto
        {
            Email = "test@example.com",
            Password = "password"
        };
        var accessToken = "access-token";
        var refreshToken = "refresh-token";

        _userRepositoryMock.Setup(repo => repo.GetByEmail(loginDto.Email)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("password", "hashedPassword", "salt")).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateJwtToken(user)).Returns(accessToken);
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns(refreshToken);

        var result = await authService.Login(loginDto);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(accessToken, result.Value.AccessToken);
        Assert.Equal(refreshToken, result.Value.RefreshToken);

        _cacheMock.Verify(c => c.StoreOnCache(It.IsAny<string>(), refreshToken, TimeSpan.FromDays(30)), Times.Once);
    }
    [Fact]
    public async Task Login_ShouldReturnError_WhenCredentialsAreInvalid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Password = "hashedPassword",
            Salt = "salt",
        };
        var loginDto = new UserLoginDto
        {
            Email = "wrongmail@mail.com",
            Password = "wrongpassword"
        };

        _userRepositoryMock.Setup(repo => repo.GetByEmail(loginDto.Email)).ReturnsAsync((User?)null);

        var result = await authService.Login(loginDto);

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Login_ShouldReturnError_WhenCredentialsIsMissing()
    {
        var dto = new UserLoginDto();
        _userRepositoryMock.Setup(repo => repo.GetByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);

        var result = await authService.Login(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNewAccessToken_WhenValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Password = "hashedPassword"
        };
        var tokenDto = new TokenDto("expired-acess-token", "refresh-token");
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var newAccessToken = "new-access-token";

        _tokenServiceMock.Setup(t => t.GetPrincipalFromExpiredToken(tokenDto.AccessToken!)).Returns(principal);
        _userRepositoryMock.Setup(repo => repo.GetById(user.Id)).ReturnsAsync(user);
        _cacheMock.Setup(c => c.GetFromCache(It.IsAny<string>())).ReturnsAsync(tokenDto.RefreshToken ?? "default-refresh-token");
        _tokenServiceMock.Setup(t => t.GenerateJwtToken(user)).Returns(newAccessToken);

        var result = await authService.RefreshToken(tokenDto.RefreshToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(newAccessToken, result.Value.AccessToken);
        Assert.Equal(tokenDto.RefreshToken, result.Value.RefreshToken);
    }

    [Fact]
    public async Task ResetPassword_ShouldUpdatePassword_WhenTokenIsValid()
    {

        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Password = "oldPassword", Salt = "oldSalt" };
        var token = "reset-token";
        var newPassword = "newPassword";
        var salt = new byte[16];
        var hashedPassword = "hashedPassword";

        _cacheMock.Setup(c => c.GetFromCache(It.IsAny<string>())).ReturnsAsync(userId.ToString());
        _userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.GenerateSalt()).Returns(salt);
        _hasherMock.Setup(h => h.HashPassword(newPassword, salt)).Returns(hashedPassword);

        var result = await authService.ResetPassword(newPassword, token);

        Assert.True(result.IsSuccess);
        Assert.Equal("Email was sent if success!", result.Value);
        Assert.Equal(hashedPassword, user.Password);
        Assert.Equal(Convert.ToBase64String(salt), user.Salt);

        _userRepositoryMock.Verify(repo => repo.Update(user), Times.Once);
    }
}

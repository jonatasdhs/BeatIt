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
        // Arrange
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
        var refreshToken = $"refresh-token";

        _userRepositoryMock
            .Setup(repo => repo.GetByEmail(loginDto.Email))
            .ReturnsAsync(user);
        _hasherMock
            .Setup(h => h.VerifyPassword(loginDto.Password, user.Password, user.Salt))
            .Returns(true);
        _tokenServiceMock
            .Setup(t => t.GenerateJwtToken(user))
            .Returns(accessToken);
        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns(refreshToken);
        // Act
        var result = await authService.Login(loginDto);
        // Assert        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(accessToken, result.Value.AccessToken);

        _cacheMock.Verify(c => c.StoreOnCache(It.IsAny<string>(), refreshToken, TimeSpan.FromDays(30)), Times.Once);
    }
    [Fact]
    public async Task Login_ShouldReturnError_WhenCredentialsAreInvalid()
    {
        // Arrange
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
        // Act
        var result = await authService.Login(loginDto);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Login_ShouldReturnError_WhenCredentialsIsMissing()
    {
        // Arrange
        var dto = new UserLoginDto();
        _userRepositoryMock.Setup(repo => repo.GetByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);
        // Act
        var result = await authService.Login(dto);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnSuccess_WhenTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = $"{userId}:valid_refresh_token";
        var user = new User
        {
            Id = userId,
            Email = "testuser@example.com"
        };

        _userRepositoryMock.Setup(repo => repo.GetById(userId))
            .ReturnsAsync(user);
        _cacheMock.Setup(cache => cache.GetAsync($"userId: {userId}"))
            .ReturnsAsync("valid_refresh_token");
        _tokenServiceMock.Setup(service => service.GenerateJwtToken(user))
            .Returns("new_access_token");
        _tokenServiceMock.Setup(service => service.GenerateRefreshToken())
            .Returns("new_refresh_token");

        // Act
        var result = await authService.RefreshToken(refreshToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new_access_token", result.Value.AccessToken);
        _cacheMock.Verify(cache => cache.StoreOnCache($"userId: {userId}", "new_refresh_token", TimeSpan.FromDays(30)), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ShouldUpdatePassword_WhenTokenIsValid()
    {
        // Arrange

        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Password = "oldPassword", Salt = "oldSalt" };
        var token = "reset-token";
        var newPassword = "newPassword";
        var salt = new byte[16];
        var hashedPassword = "hashedPassword";

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(userId.ToString());
        _userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.GenerateSalt()).Returns(salt);
        _hasherMock.Setup(h => h.HashPassword(newPassword, salt)).Returns(hashedPassword);
        // Act
        var result = await authService.ResetPassword(newPassword, token);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Email was sent if success!", result.Value);
        Assert.Equal(hashedPassword, user.Password);
        Assert.Equal(Convert.ToBase64String(salt), user.Salt);

        _userRepositoryMock.Verify(repo => repo.Update(user), Times.Once);
    }
}

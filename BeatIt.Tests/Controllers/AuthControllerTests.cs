using BeatIt.Controllers;
using BeatIt.Errors;
using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Services.AuthService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace BeatIt.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;
    private readonly ITestOutputHelper _output;
    public AuthControllerTests(ITestOutputHelper output)
    {
        _output = output;
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_WhenValid_ShouldReturnTokens()
    {
        // Arrange
        var tokenDto = new TokenDto("access-token", "refresh-token");
        var loginDto = new UserLoginDto { Email = "test@mail.com", Password = "test" };
        _authServiceMock
            .Setup(service => service.Login(loginDto))
            .ReturnsAsync(Result.Success(tokenDto));
        var context = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext = context;

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsType<Ok>(result);

        var cookies = context.Response.Headers.SetCookie;
        Assert.Contains(cookies, c => c!.Contains("AccessToken=access-token"));
        Assert.Contains(cookies, c => c!.Contains("RefreshToken=refresh-token"));
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShouldReturnProblemDetails()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "test@mail.com", Password = "asdhasuidh" };
        _authServiceMock
            .Setup(service => service.Login(loginDto))
            .ReturnsAsync(Result.Failure<TokenDto>(AuthErrors.NotFound));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(404, problemDetails.StatusCode);
        Assert.Contains("Not Found", problemDetails.ProblemDetails.Title);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnAndSetTokens()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var cookieMock = new Dictionary<string, string>
        {
            { "RefreshToken", "validToken" }
        };
        var tokenDto = new TokenDto("newAccessToken", "newRefreshToken");
        context.Request.Headers.Cookie = string.Join("; ", cookieMock.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        _controller.ControllerContext.HttpContext = context;

        _authServiceMock
            .Setup(service => service.RefreshToken("validToken"))
            .ReturnsAsync(tokenDto);

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        Assert.IsType<NoContent>(result);

        var cookies = context.Response.Headers.SetCookie;
        Assert.Contains(cookies, c => c!.Contains("AccessToken=newAccessToken"));
        Assert.Contains(cookies, c => c!.Contains("RefreshToken=newRefreshToken"));
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_ReturnsProblemDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var cookieMock = new Dictionary<string, string>
    {
        { "RefreshToken", "invalidToken" }
    };
        context.Request.Headers["Cookie"] = string.Join("; ", cookieMock.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        _controller.ControllerContext.HttpContext = context;

        var failureResult = Result.Failure<TokenDto>(AuthErrors.InvalidToken);
        _authServiceMock.Setup(a => a.RefreshToken("invalidToken")).ReturnsAsync(failureResult);

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        var problemDetails = Assert.IsType<BadRequest<Error>>(result);
        Assert.Equal(400, problemDetails.StatusCode);
        Assert.Contains("Token is invalid", problemDetails!.Value!.Description.ToString());
    }

}
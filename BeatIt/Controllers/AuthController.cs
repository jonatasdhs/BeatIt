using BeatIt.Extensions;
using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Services.AuthService;
using Microsoft.AspNetCore.Mvc;

namespace BeatIt.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(IAuthService authInterface, ILogger<AuthController> logger) : ControllerBase
{
    private readonly IAuthService _authInterface = authInterface;
    private readonly ILogger<AuthController> _logger = logger;
    [HttpPost("login")]
    public async Task<IResult> Login([FromBody] UserLoginDto login)
    {

        _logger.LogInformation("Login attempt for user: {user}", login.Email);

        Result<TokenDto> result = await _authInterface.Login(login);

        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        Response.Cookies.Append("AccessToken", result.Value!.AccessToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTime.UtcNow.ToLocalTime().AddMinutes(15)
        });
        Response.Cookies.Append("RefreshToken", result.Value!.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTime.UtcNow.ToLocalTime().AddDays(30)
        });
        _logger.LogInformation(AppLogEvents.LoginSuccessful, "Login successful for user: {}", login.Email);
        return Results.Ok();
    }


    [HttpPost("refreshtoken")]
    public async Task<IResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["RefreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning(AppLogEvents.LoginFailed, "Error : Refresh token or access token is missing");
            return Results.Unauthorized();
        }

        Result<TokenDto> result = await _authInterface.RefreshToken(refreshToken);
        if (result.IsFailure)
        {
            _logger.LogWarning(AppLogEvents.LoginFailed, "Error: {error}", result.Error);
            return result.ToProblemDetails();
        }

        Response.Cookies.Append("AccessToken", result.Value!.AccessToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });
        Response.Cookies.Append("RefreshToken", result.Value!.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

        _logger.LogInformation(AppLogEvents.LoginSuccessful, "Success: Refresh token is valid");
        return Results.NoContent();
    }
}

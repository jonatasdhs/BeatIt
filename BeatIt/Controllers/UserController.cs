using BeatIt.Extensions;
using BeatIt.Filters;
using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Repositories;
using BeatIt.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeatIt.Controllers;

[Route("api/users")]
[ApiController]
[ServiceFilter(typeof(UserAuthenticationFilter))]
public class UserController(IUserService userService, IUserRepository userRepository, ILogger<UserController> logger) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger _logger = logger;

    [HttpPost("register")]
    public async Task<IResult> CreateUser([FromBody] UserCreateDto newUser)
    {

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
            _logger.LogWarning(AppLogEvents.InvalidValidation, "Bad request {errors}", errors);
            return Results.BadRequest();
        }

        var result = await _userService.CreateUser(newUser);

        if (result.IsFailure)
        {
            _logger.LogWarning(AppLogEvents.Error, "Failed to create user {error}", result.ToProblemDetails());
            return result.ToProblemDetails();
        }
        _logger.LogInformation(AppLogEvents.Create, "User created: {user}", result.Value.Email);
        return Results.Json(result.Value);
    }

    [Authorize]
    [HttpPatch()]
    public async Task<IResult> UpdateUser([FromBody] UserUpdateDto updatedUser)
    {
        if (!ModelState.IsValid)
        {
            return Results.BadRequest();
        }
        if (HttpContext.Items["AuthenticateUser"] is not User user)
        {
            return Results.BadRequest();
        }
        var result = await _userService.UpdateUser(updatedUser, user.Id);
        if (result.IsFailure)
        {
            return Results.NotFound(result.Error);
        }
        return Results.NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IResult> DeleteUser(Guid id)
    {
        var user = await _userRepository.GetById(id);
        if (user is null)
        {
            return Results.NotFound();
        }

        await _userService.SoftDelete(user.Id);
        return Results.NoContent();
    }
}

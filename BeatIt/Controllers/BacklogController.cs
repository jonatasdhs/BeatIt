using BeatIt.Extensions;
using BeatIt.Filters;
using BeatIt.Models;
using BeatIt.Repositories;
using BeatIt.Services.BacklogService;
using BeatIt.Services.TokenService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeatIt.Controllers;

[Route("api/backlog")]
[ApiController]
[ServiceFilter(typeof(UserAuthenticationFilter))]
public class BacklogController: ControllerBase
{
    private readonly IBacklogService _backlogService;

    public BacklogController(IBacklogService backlogService)
    {
        _backlogService = backlogService;
    }

    [Authorize]
    [HttpPost("{gameId}")]
    public async Task<IResult> AddBacklogGame(int gameId)
    {
        if (HttpContext.Items["AuthenticateUser"] is not User user)
        {
            return Results.BadRequest();
        }
        var result = await _backlogService.AddGameToBacklog(user, gameId);
        if(result.IsFailure)
        {
            return result.ToProblemDetails();
        }
        var resourceUri = $"/api/backlog/{gameId}";
        return Results.Created(resourceUri, result);
    }

    [Authorize]
    [HttpDelete("{gameId}")]
    public async Task<IResult> RemoveBacklogGame(int gameId)
    {
        if (HttpContext.Items["AuthenticateUser"] is not User user)
        {
            return Results.BadRequest();
        }
        var result = await _backlogService.RemoveGameFromBacklog(user.Id, gameId);
        if(result.IsFailure)
        {
            return result.ToProblemDetails();
        }
        return Results.NoContent();
    }

    [Authorize]
    [HttpGet("")]
    public async Task<IResult> GetAllBacklogsFromUser()
    {
        if (HttpContext.Items["AuthenticateUser"] is not User user)
        {
            return Results.BadRequest();
        }
        var result = await _backlogService.GetAllBacklogsFromUsers(user);
        if(result.IsFailure)
        {
            return result.ToProblemDetails();
        }
        return Results.Ok(result);
    }
}
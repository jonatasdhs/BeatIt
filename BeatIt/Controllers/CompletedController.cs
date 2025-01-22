using BeatIt.Extensions;
using BeatIt.Models.DTOs;
using BeatIt.Services.BacklogService; 
using BeatIt.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BeatIt.Models;

namespace BeatIt.Controllers;

[Route("api/completed")]
[ApiController]
[ServiceFilter(typeof(UserAuthenticationFilter))]
public class CompletedController(ICompletedService completedService) : ControllerBase
{
    private readonly ICompletedService _completedService = completedService;

    [Authorize]
    [HttpPost("{gameId}")]
    public async Task<IResult> AddGameToCompletedList([FromBody] CompletedGameDto completedGame, int gameId)
    {
        if (!ModelState.IsValid)
        {
            return Results.BadRequest();
        }
        if (HttpContext.Items["AuthenticateUser"] is not User user)
        {
            return Results.BadRequest();
        }
        var result = await _completedService.AddGameToCompletedList(user, gameId, completedGame);
        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }
        var resourceUri = $"/api/completed/{gameId}";

        return Results.Created(resourceUri, result);
    }

    [Authorize]
    [HttpGet()]
    public async Task<IResult> GetAllCompletedGames()
    {
        if (HttpContext.Items["AuthenticateUser"] is not User user)
        {
            return Results.BadRequest();
        }
        var result = await _completedService.GetAllCompletedGames(user);
        return Results.Json(result);
    }
}
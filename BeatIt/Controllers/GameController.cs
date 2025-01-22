using BeatIt.Errors;
using BeatIt.Extensions;
using BeatIt.Filters;
using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Repositories;
using BeatIt.Services.BacklogService;
using BeatIt.Services.GameService;
using BeatIt.Services.TokenService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeatIt.Controllers;

[Route("api/games")]
[ApiController]
[ServiceFilter(typeof(UserAuthenticationFilter))]
public class GameController(IGameService gameInterface, IBacklogService backlogService, ITokenService tokenService, ILogger<GameController> logger, IUserRepository userRepository) : ControllerBase
{
    private readonly IGameService _gameInterface = gameInterface;
    private readonly IBacklogService _backlogService = backlogService;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger<GameController> _logger = logger;

    [Authorize]
    [HttpGet()]
    public async Task<IResult> GetAllGames()
    {
       if (HttpContext.Items["AuthenticateUser"] is not User user)
        {
            return Results.BadRequest();
        }
        var games = await _gameInterface.GetAllGames();
        return Results.Ok(games);
    }
}
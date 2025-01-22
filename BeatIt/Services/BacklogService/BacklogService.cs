using BeatIt.Errors;
using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Repositories;
using BeatIt.Services.GameService;

namespace BeatIt.Services.BacklogService;

public class BacklogService(IGameRepository gameRepository, IGameService gameService, ILogger<BacklogService> logger) : IBacklogService
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGameService _gameService = gameService;
    private readonly ILogger<BacklogService> _logger = logger;

    public async Task<Result<BacklogResponse>> AddGameToBacklog(User user, int gameId)
    {
        var gameResult = await _gameService.GetOrCreateGame(gameId);
        if (gameResult.IsFailure)
        {
            return Result.Failure<BacklogResponse>(BacklogErrors.NotFound);
        }
        var gameExists = await _gameRepository.BacklogGameExists(gameResult.Value, user.Id);
        if (gameExists == true)
        {
            _logger.LogDebug("Game exists: {gameExists}", gameExists);
            return Result.Failure<BacklogResponse>(BacklogErrors.Conflict);
        }


        var backlog = new Backlog
        {
            UserId = user.Id,
            GameId = gameResult.Value.Id,
            Created_at = DateTime.Now.ToLocalTime()
        };

        await _gameRepository.AddGameToBacklog(backlog);
        return Result.Success(new BacklogResponse
        {
            GameName = gameResult.Value.Name,
            Created_at = backlog.Created_at,
        });
    }

    public async Task<Result<List<BacklogResponse>>> GetAllBacklogsFromUsers(User user)
    {
        var backlogs = await _gameRepository.GetAllBacklogWithGame(user.Id);
        return Result.Success(backlogs);
    }

    public async Task<Result<string>> RemoveGameFromBacklog(Guid userId, int gameId)
    {
        var game = await _gameRepository.GetById(gameId);
        if (game is null)
        {
            return Result.Failure<string>(GameErrors.NotFound);
        }
        var gameExists = await _gameRepository.BacklogGameExists(game, userId);
        if (gameExists == false)
        {
            return Result.Failure<string>(BacklogErrors.NotFound);
        }
        var backlog = await _gameRepository.GetBacklogByIdAndUserId(game.Id, userId);
        if (backlog == null)
        {
            return Result.Failure<string>(BacklogErrors.NotFound);
        }

        await _gameRepository.RemoveGameFromBacklog(backlog!);
        return Result.Success("Game removed from backlog with success!");
    }
}
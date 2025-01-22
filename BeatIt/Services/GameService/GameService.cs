using BeatIt.Errors;
using BeatIt.Models;
using BeatIt.Repositories;
using BeatIt.Services.IGDBService;

namespace BeatIt.Services.GameService;

public class GameService(IIgdbService igdbService, ILogger<GameService> logger, IGameRepository gameRepository) : IGameService
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IIgdbService _igdbService = igdbService;
    private readonly ILogger<GameService> _logger = logger;

    public async Task<Result<Game>> GetOrCreateGame(int gameId)
    {
        var game = await _gameRepository.GetById(gameId);
        if (game is null)
        {
            var igdbGame = await _igdbService.GetGameFromIgdbAsync(gameId)!;
            if (igdbGame == null)
            {
                _logger.LogWarning(AppLogEvents.ReadNotFound, "Game not found in IgDB");
                return Result.Failure<Game>(GameErrors.NotFound);
            }
            game = new Game
            {
                IgdbGameId = igdbGame.Id,
                Name = igdbGame!.Name!
            };
            await _gameRepository.AddGame(game);
        }
        return Result.Success(game);
    }

    public async Task<Result<List<Game>>> GetAllGames()
    {
        var games = await _gameRepository.GetAllGames();
        return Result.Success<List<Game>>(games);
    }
}
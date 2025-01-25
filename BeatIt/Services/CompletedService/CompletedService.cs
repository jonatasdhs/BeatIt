using BeatIt.Errors;
using BeatIt.Models;
using BeatIt.Models.DTOs;
using BeatIt.Repositories;
using BeatIt.Services.BacklogService;
using BeatIt.Services.GameService;

namespace BeatIt.Services.CompletedService;

public class CompletedService(IGameRepository gameRepository, IGameService gameService) : ICompletedService
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IGameService _gameService = gameService;
    public async Task<Result<string>> AddGameToCompletedList(User user, int gameId, CompletedGameDto completedGame)
    {
        var gameResult = await _gameService.GetOrCreateGame(gameId);
        
        if (gameResult.IsFailure)
        {
            return Result.Failure<string>(GameErrors.NotFound);
        }
        var gameExists = await _gameRepository.CompletedGameExists(gameResult.Value, user.Id);
        if (gameExists)
        {
            return Result.Failure<string>(GameErrors.Conflict);
        }
        CompletedGames newCompletedGame = new()
        {
            UserId = user.Id,
            GameId = gameResult.Value.Id,
            Rating = completedGame.Rating,
            Notes = completedGame.Notes,
            TimeToComplete = completedGame.TimeToComplete,
            FinishedDate = completedGame.FinishedDate,
            Platform = completedGame.Platform,
            Difficulty = completedGame.Difficulty,
            StartDate = completedGame.StartDate
        };
        await _gameRepository.AddGameToCompletedGames(newCompletedGame);
        return Result.Success("Game added with success");
    }

    public async Task<Result<List<CompletedGames>>> GetAllCompletedGames(User user)
    {
        var completedGames = await _gameRepository.GetAllCompletedGames(user.Id);
        return Result.Success(completedGames);
    }
}
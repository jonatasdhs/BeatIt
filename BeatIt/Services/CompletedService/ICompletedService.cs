using BeatIt.Models;
using BeatIt.Models.DTOs;

namespace BeatIt.Services.BacklogService;

public interface ICompletedService
{
    Task<Result<string>> AddGameToCompletedList(User user, int gameId, CompletedGameDto completedGame);
    Task<Result<List<CompletedGames>>> GetAllCompletedGames(User user);
}
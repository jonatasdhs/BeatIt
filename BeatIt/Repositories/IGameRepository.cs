using BeatIt.Models;
using BeatIt.Models.DTOs;

namespace BeatIt.Repositories;

public interface IGameRepository
{
    Task<Game?> GetById(int id);
    Task<bool> CompletedGameExists(Game game, Guid userId);
    Task<bool> BacklogGameExists(Game game, Guid userId);
    Task<Game> AddGame(Game game);
    Task AddGameToBacklog(Backlog backlog);
    Task AddGameToCompletedGames(CompletedGames completedGames);
    Task RemoveGameFromBacklog(Backlog backlog);
    Task<Backlog?> GetBacklogByIdAndUserId(int gameId, Guid userId);
    Task<List<Backlog>> GetAllBacklog(Guid userId);
    Task<List<BacklogResponse>> GetAllBacklogWithGame(Guid userId);
    Task<List<CompletedGames>> GetAllCompletedGames(Guid userId);
    Task<List<Game>> GetAllGames();
}
using BeatIt.Models.DTOs;
using BeatIt.Models;

namespace BeatIt.Services.GameService
{
    public interface IGameService
    {
        Task<Result<List<Game>>> GetAllGames();
        Task<Result<Game>> GetOrCreateGame(int gameId);
    }
}
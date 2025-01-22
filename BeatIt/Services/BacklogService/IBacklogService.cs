using BeatIt.Models;
using BeatIt.Models.DTOs;

namespace BeatIt.Services.BacklogService;

public interface IBacklogService
{
    Task<Result<BacklogResponse>> AddGameToBacklog(User user, int gameId);
    Task<Result<string>> RemoveGameFromBacklog(Guid userId, int gameId);
    Task<Result<List<BacklogResponse>>> GetAllBacklogsFromUsers(User user);
}
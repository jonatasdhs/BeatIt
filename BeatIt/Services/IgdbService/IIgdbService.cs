using BeatIt.Models;
using BeatIt.Services.IgdbService;

namespace BeatIt.Services.IGDBService;

public interface IIgdbService
{
    Task<IgdbResponse?> GetGameFromIgdbAsync(int gameId);
    Task<string> GetIgdbAccessTokenAsync();
    Task<CustomTokenResponse> RequestIgdbTokenAsync();
}
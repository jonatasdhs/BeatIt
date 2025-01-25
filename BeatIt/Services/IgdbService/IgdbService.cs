using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeatIt.Services.IGDBService;
using BeatIt.Services.CacheService;
using Microsoft.Extensions.Options;
using BeatIt.Utils;

namespace BeatIt.Services.IgdbService;

public class IgdbService : IIgdbService
{
    private readonly ICacheService _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IgdbConfig _config;
    private readonly ILogger<IgdbService> _logger;
    private readonly string _igdbClientId;
    private readonly string _igdbSecretKey;
    public IgdbService(ICacheService cache, IHttpClientFactory httpClientFactory, IOptions<IgdbConfig> config, ILogger<IgdbService> logger)
    {
        _config = config.Value;
        _igdbClientId = _config.ClientId ?? throw new InvalidOperationException("Igdb client_id is not set");
        _igdbSecretKey = _config.SecretKey ?? throw new InvalidOperationException("Igdb secret_key is not set");
        _logger = logger;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
    }
    public async Task<IgdbResponse?> GetGameFromIgdbAsync(int gameId)
    {
        _logger.LogInformation("Fetching game details for GameId: {GameId} from IGDB", gameId);
        var body = $"fields id, name; where id = {gameId};";
        var requestUri = "https://api.igdb.com/v4/games/";
        StringContent content = new(body, Encoding.UTF8, "text/plain");
        var accessToken = await GetIgdbAccessTokenAsync();
        HttpClient client = _httpClientFactory.CreateClient();

        _logger.LogDebug("Setting up HTTP headers for IGDB request.");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Client-Id", _igdbClientId);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        _logger.LogInformation("Sending request to IGDB for GameId: {GameId}", gameId);
        var response = await client.PostAsync(requestUri, content);

        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Successfully fetched data for GameId: {GameId}", gameId);

        var jsonResponse = await response.Content.ReadAsByteArrayAsync();
        var game = JsonSerializer.Deserialize<IgdbResponse[]>(jsonResponse);

        if (game == null || game.Length == 0)
        {
            _logger.LogWarning("No game found for GameId: {GameId}", gameId);
            return null;
        }

        return game[0];
    }

    public async Task<string> GetIgdbAccessTokenAsync()
    {
        _logger.LogInformation("Attempting to retrieve IGDB access token from cache.");

        var accessToken = await _cache.GetAsync("IGDB_AccessToken");
        if (accessToken != null)
        {
            _logger.LogInformation("IGDB access token retrieved from cache.");
            return accessToken;
        }

        _logger.LogInformation("Access token not found in cache. Requesting a new token.");
        var tokenResponse = await RequestIgdbTokenAsync();

        _logger.LogInformation("Storing new access token in cache.");
        await _cache.StoreOnCache("IGDB_AccessToken", tokenResponse.AccessToken!, TimeSpan.FromSeconds(tokenResponse.ExpiresIn));

        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Access token cannot be obtained");
    }

    public async Task<CustomTokenResponse> RequestIgdbTokenAsync()
    {
        var requestUri = $"https://id.twitch.tv/oauth2/token?client_id={_igdbClientId}&client_secret={_igdbSecretKey}&grant_type=client_credentials";
        _logger.LogInformation("Requesting IGDB token from Twitch API: {Uri}", requestUri);

        using HttpClient client = _httpClientFactory.CreateClient();

        var response = await client.PostAsync(requestUri, null);
        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Successfully retrieved IGDB token.");

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<CustomTokenResponse>(content);

        return tokenResponse ?? throw new InvalidOperationException("Access token cannot be obtained");
    }


}

public class IgdbResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
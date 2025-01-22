using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeatIt.Models;
using BeatIt.Services.IGDBService;
using BeatIt.Services.TokenService;
using BeatIt.Services.CacheService;

namespace BeatIt.Services.IgdbService;

public class IgdbService : IIgdbService
{
    private readonly ITokenService _tokenService;
    private readonly ICacheService _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _igdbClientId;
    private readonly string _igdbSecretKey;
    private readonly string _clientId = Environment.GetEnvironmentVariable("IGDB_CLIENT_ID")!;
    public IgdbService(ITokenService tokenService, ICacheService cache, IHttpClientFactory httpClientFactory)
    {
        _igdbClientId = Environment.GetEnvironmentVariable("IGDB_CLIENT_ID")!;
        _igdbSecretKey = Environment.GetEnvironmentVariable("IGDB_SECRET_KEY")
                 ?? throw new InvalidOperationException("IGDB_SECRET_KEY is not set.");
        _tokenService = tokenService;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
    }
    public async Task<IgdbResponse?> GetGameFromIgdbAsync(int gameId)
    {
        var body = $"fields id, name; where id = {gameId};";
        var requestUri = "https://api.igdb.com/v4/games/";
        StringContent content = new(body, Encoding.UTF8, "text/plain");
        var accessToken = await GetIgdbAccessTokenAsync();
        HttpClient client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Client-Id", _clientId);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsByteArrayAsync();

        var game = JsonSerializer.Deserialize<IgdbResponse[]>(jsonResponse);

        return game!.Length > 0 ? game[0] : null;
    }

    public async Task<string> GetIgdbAccessTokenAsync()
    {
        var accessToken = await _cache.GetFromCache("IGDB_AccessToken");
        if (accessToken != null)
        {
            return accessToken;
        }
        var tokenResponse = await RequestIgdbTokenAsync();
        await _cache.StoreOnCache("IGDB_AccessToken", tokenResponse.AccessToken!, TimeSpan.FromSeconds(tokenResponse.ExpiresIn));
        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Access token cannot be obtained");
    }

    public async Task<CustomTokenResponse> RequestIgdbTokenAsync()
    {
        var requestUri = $"https://id.twitch.tv/oauth2/token?client_id={_igdbClientId}&client_secret={_igdbSecretKey}&grant_type=client_credentials";
        using HttpClient client = _httpClientFactory.CreateClient();
        var response = await client.PostAsync(requestUri, null);
        response.EnsureSuccessStatusCode();

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
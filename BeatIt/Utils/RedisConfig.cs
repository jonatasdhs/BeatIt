using StackExchange.Redis;

namespace BeatIt.Utils;

public class RedisConfiguration
{
    public required string Endpoint { get; set; }
    public required string Password { get; set; }
    public int SyncTimeout { get; set; }
}
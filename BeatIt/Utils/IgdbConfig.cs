using BeatIt.Models;
using Microsoft.Extensions.Options;

namespace BeatIt.Utils;

public class IgdbConfig
{
    /* private readonly IgdbOptions _options; */
    public string SecretKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
}


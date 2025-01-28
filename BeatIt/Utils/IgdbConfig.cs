using BeatIt.Models;
using Microsoft.Extensions.Options;

namespace BeatIt.Utils;

public class IgdbConfig
{
    public string SECRET_KEY { get; set; } = string.Empty;
    public string CLIENT_ID { get; set; } = string.Empty;
}


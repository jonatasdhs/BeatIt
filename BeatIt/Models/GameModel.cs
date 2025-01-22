using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BeatIt.Models;

public class Game
{
    [Key]
    public int Id { get; set; }
    public int IgdbGameId { get; set; }
    public required string Name { get; set; }
    [JsonIgnore]
    public ICollection<Backlog>? Backlogs { get; set; }
    [JsonIgnore]
    public ICollection<CompletedGames>? CompletedGames { get; set; }
}
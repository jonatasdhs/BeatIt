using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BeatIt.Models;

public class CompletedGames
{
    [Key]
    public int Id { get; set; }
    public int GameId { get; set; }
    public Guid UserId { get; set; }

    public int Difficulty { get; set; }
    public int Rating { get; set; }
    public string? Notes { get; set; }
    public DateTime FinishedDate { get; set; }
    public TimeSpan TimeToComplete { get; set; }
    public string? Platform { get; set; }
    public DateTime? StartDate { get; set; }

    [JsonIgnore]
    public Game? Game { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
}
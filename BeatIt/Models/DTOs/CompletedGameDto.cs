namespace BeatIt.Models.DTOs;

public class CompletedGameDto
{
    public int Difficulty { get; set; }
    public int Rating { get; set; }
    public string? Notes { get; set; }
    public DateTime FinishedDate { get; set; }
    public TimeSpan TimeToComplete { get; set; }
    public required string Platform { get; set; }
    public DateTime? StartDate { get; set; }
}
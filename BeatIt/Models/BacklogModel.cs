using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BeatIt.Models;

public class Backlog
{
    [Key]
    public int Id { get; set; }

    public int GameId { get; set; }
    [JsonIgnore]
    public Game? Game { get; set; }


    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
    

    public DateTime Created_at { get; set; }
}
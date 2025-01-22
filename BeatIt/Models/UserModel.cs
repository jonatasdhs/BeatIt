using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BeatIt.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsActive {get;set;} = true;
        public string Salt { get; set; } = null!;
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        [JsonIgnore]
        public ICollection<Backlog>? Backlogs { get; set; }
        [JsonIgnore]
        public ICollection<CompletedGames>? CompletedGames { get; set; }
    }
}
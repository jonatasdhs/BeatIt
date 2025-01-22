using System.ComponentModel.DataAnnotations;

namespace BeatIt.Models.DTOs
{
    public class TokenDto(string accessToken, string refreshToken)
    {
        public string? AccessToken { get; set; } = accessToken;
        public string? RefreshToken { get; set; } = refreshToken;
    }
}
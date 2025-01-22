namespace BeatIt.Services.PasswordService;

public interface IPasswordService {
    public string HashPassword(string password, byte[] salt);
    public byte[] GenerateSalt();
    public bool VerifyPassword(string password, string storedHash, string storedSalt);
}
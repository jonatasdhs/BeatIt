namespace BeatIt.Services.PasswordService;

using System.Text;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;

public class PasswordService: IPasswordService
{
    public string HashPassword(string password, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var argon2 = new Argon2id(passwordBytes)
        {
            Salt = salt,
            DegreeOfParallelism = 8,
            MemorySize = 65536,
            Iterations = 4
        };
        var hashBytes = argon2.GetBytes(32);
        return Convert.ToBase64String(hashBytes);
    }
    public byte[] GenerateSalt()
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        return salt;
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        byte[] saltByte = Convert.FromBase64String(storedSalt);
        var newHash = HashPassword(password, saltByte);
        return newHash == storedHash;
    }
}
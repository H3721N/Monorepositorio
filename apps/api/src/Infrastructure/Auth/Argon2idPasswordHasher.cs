using System.Security.Cryptography;
using System.Text;
using Application.Interfaces.Auth;
using Konscious.Security.Cryptography;

namespace Infrastructure.Auth;

public sealed class Argon2idPasswordHasher : IPasswordHasher
{
    private const int CurrentIterations = 2;
    private const int CurrentMemorySize = 32768;
    private const int LegacyIterations = 4;
    private const int LegacyMemorySize = 65536;

    public string GenerateSalt()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    }

    public string HashPassword(string password, string salt)
    {
        return HashPassword(password, salt, CurrentIterations, CurrentMemorySize);
    }

    public bool VerifyPassword(string password, string salt, string passwordHash)
    {
        return VerifyWithParameters(password, salt, passwordHash, CurrentIterations, CurrentMemorySize)
            || VerifyWithParameters(password, salt, passwordHash, LegacyIterations, LegacyMemorySize);
    }

    private static bool VerifyWithParameters(string password, string salt, string passwordHash, int iterations, int memorySize)
    {
        var computedHash = HashPassword(password, salt, iterations, memorySize);
        var computedHashBytes = Convert.FromBase64String(computedHash);
        var storedHashBytes = Convert.FromBase64String(passwordHash);

        return CryptographicOperations.FixedTimeEquals(computedHashBytes, storedHashBytes);
    }

    private static string HashPassword(string password, string salt, int iterations, int memorySize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(salt);

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltBytes = Convert.FromBase64String(salt);

        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = saltBytes,
            DegreeOfParallelism = 2,
            Iterations = iterations,
            MemorySize = memorySize
        };

        return Convert.ToBase64String(argon2.GetBytes(32));
    }
}

namespace Mro.Application.Abstractions;

/// <summary>
/// Password hashing abstraction.
/// Infrastructure implements this with BCrypt (work factor 12).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Produces a salted hash suitable for persistence.</summary>
    string Hash(string plainPassword);

    /// <summary>Compares a plain-text password against a stored hash.</summary>
    bool Verify(string plainPassword, string storedHash);
}

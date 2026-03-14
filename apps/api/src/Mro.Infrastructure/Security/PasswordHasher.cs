using Mro.Application.Abstractions;

namespace Mro.Infrastructure.Security;

/// <summary>
/// BCrypt password hasher. Work factor 12 (≈ 300 ms on a 2024 server).
/// Do not lower the work factor below 12 — see docs/architecture/security-architecture.md.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);

    public bool Verify(string plainPassword, string storedHash) =>
        BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
}

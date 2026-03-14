using Mro.Domain.Aggregates.User;

namespace Mro.Application.Abstractions;

/// <summary>
/// Repository contract for the User aggregate.
/// Implemented in Mro.Infrastructure; injected into Application handlers.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lookup by email is always scoped to an organisation (unique per org).</summary>
    Task<User?> GetByEmailAsync(string email, Guid organisationId, CancellationToken ct = default);

    /// <summary>Used by the refresh-token rotation flow.</summary>
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);

    Task UpdateAsync(User user, CancellationToken ct = default);
}

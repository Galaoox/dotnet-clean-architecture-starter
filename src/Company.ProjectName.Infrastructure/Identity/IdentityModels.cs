using Microsoft.AspNetCore.Identity;

namespace Company.ProjectName.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
}

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid userId,
        string tokenHash,
        string familyId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        Id = Guid.CreateVersion7();
        UserId = userId;
        TokenHash = tokenHash;
        FamilyId = familyId;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public string FamilyId { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public ApplicationUser User { get; private set; } = null!;

    public bool IsActive(DateTimeOffset now)
        => RevokedAtUtc is null && ExpiresAtUtc > now;

    public void Revoke(DateTimeOffset now, string? replacedByTokenHash = null)
    {
        RevokedAtUtc ??= now;
        ReplacedByTokenHash ??= replacedByTokenHash;
    }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        string? familyId = null)
        => new(userId, tokenHash, familyId ?? Guid.CreateVersion7().ToString("N"), createdAtUtc, expiresAtUtc);
}

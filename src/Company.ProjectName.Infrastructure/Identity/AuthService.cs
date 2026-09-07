using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Company.ProjectName.Application.Auth;
using Company.ProjectName.Domain.Common;
using Company.ProjectName.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JsonWebTokenHandler = Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler;
using Microsoft.AspNetCore.WebUtilities;

namespace Company.ProjectName.Infrastructure.Identity;

internal sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ApplicationDbContext dbContext,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider) : IAuthService
{
    private const string UserRole = "User";
    private const string AdminRole = "Admin";

    private JwtOptions Options => jwtOptions.Value;

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return Result.Failure<AuthResponse>(AuthErrors.EmailAlreadyRegistered);
        }

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return Result.Failure<AuthResponse>(AuthErrors.InvalidRegistration);
        }

        await EnsureRoleExistsAsync(UserRole);
        await userManager.AddToRoleAsync(user, UserRole);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(normalizedEmail);

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);
        }

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponse>> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            return Result.Failure<AuthResponse>(AuthErrors.InvalidRefreshToken);
        }

        if (!storedToken.IsActive(now))
        {
            await RevokeFamilyAsync(storedToken.FamilyId, now, cancellationToken);
            return Result.Failure<AuthResponse>(AuthErrors.InvalidRefreshToken);
        }

        var rawRefreshToken = CreateRawRefreshToken();
        var nextHash = HashToken(rawRefreshToken);
        storedToken.Revoke(now, nextHash);

        var nextToken = RefreshToken.Create(
            storedToken.UserId,
            nextHash,
            now,
            now.AddDays(Options.RefreshTokenDays),
            storedToken.FamilyId);

        dbContext.RefreshTokens.Add(nextToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(await BuildAuthResponseAsync(storedToken.User, rawRefreshToken, cancellationToken));
    }

    public async Task<Result> RevokeAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            return Result.Failure(AuthErrors.InvalidRefreshToken);
        }

        storedToken.Revoke(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<CurrentUserResponse>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result.Failure<CurrentUserResponse>(AuthErrors.UserNotFound);
        }

        var roles = await userManager.GetRolesAsync(user);
        return Result.Success(new CurrentUserResponse(user.Id, user.Email!, roles.ToArray()));
    }

    private async Task<Result<AuthResponse>> CreateAuthResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var rawRefreshToken = CreateRawRefreshToken();
        var now = timeProvider.GetUtcNow();
        var storedToken = RefreshToken.Create(
            user.Id,
            HashToken(rawRefreshToken),
            now,
            now.AddDays(Options.RefreshTokenDays));

        dbContext.RefreshTokens.Add(storedToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(await BuildAuthResponseAsync(user, rawRefreshToken, cancellationToken));
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(
        ApplicationUser user,
        string rawRefreshToken,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var accessTokenExpiresAt = now.AddMinutes(Options.AccessTokenMinutes);
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = CreateAccessToken(user, roles, now, accessTokenExpiresAt);

        return new AuthResponse(
            accessToken,
            accessTokenExpiresAt,
            rawRefreshToken,
            now.AddDays(Options.RefreshTokenDays),
            user.Id,
            user.Email!,
            roles.ToArray());
    }

    private string CreateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = Options.Issuer,
            Audience = Options.Audience,
            IssuedAt = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SigningKey)),
                SecurityAlgorithms.HmacSha256),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private async Task EnsureRoleExistsAsync(string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Could not create role '{role}'.");
            }
        }
    }

    private async Task RevokeFamilyAsync(
        string familyId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var family = await dbContext.RefreshTokens
            .Where(token => token.FamilyId == familyId && token.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in family)
        {
            token.Revoke(now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string CreateRawRefreshToken()
        => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

    private static string HashToken(string rawToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}

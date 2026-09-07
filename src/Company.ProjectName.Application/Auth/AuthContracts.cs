using Company.ProjectName.Domain.Common;

namespace Company.ProjectName.Application.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CurrentUserResponse>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public sealed record RegisterRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    Guid UserId,
    string Email,
    IReadOnlyList<string> Roles);

public sealed record CurrentUserResponse(Guid UserId, string Email, IReadOnlyList<string> Roles);

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Auth.InvalidCredentials",
        "The provided credentials are invalid.");

    public static readonly Error EmailAlreadyRegistered = Error.Conflict(
        "Auth.EmailAlreadyRegistered",
        "The email is already registered.");

    public static readonly Error InvalidRegistration = Error.Validation(
        "Auth.InvalidRegistration",
        "The registration data is invalid.");

    public static readonly Error InvalidRefreshToken = Error.Unauthorized(
        "Auth.InvalidRefreshToken",
        "The refresh token is invalid, expired, revoked, or already used.");

    public static readonly Error UserNotFound = Error.NotFound(
        "Auth.UserNotFound",
        "The authenticated user was not found.");
}

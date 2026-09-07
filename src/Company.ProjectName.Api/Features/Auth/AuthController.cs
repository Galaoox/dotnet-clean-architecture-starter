using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Company.ProjectName.Api.Common.Errors;
using Company.ProjectName.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.ProjectName.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult> Refresh(
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("revoke")]
    [AllowAnonymous]
    public async Task<ActionResult> Revoke(
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RevokeAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> Me(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(subject, out var userId))
        {
            return Unauthorized();
        }

        var result = await authService.GetCurrentUserAsync(userId, cancellationToken);
        return this.ToActionResult(result);
    }
}

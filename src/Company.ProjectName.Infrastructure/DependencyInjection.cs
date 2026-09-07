using Company.ProjectName.Application.Auth;
using Company.ProjectName.Application.Products;
using Company.ProjectName.Infrastructure.Identity;
using Company.ProjectName.Infrastructure.Persistence;
using Company.ProjectName.Infrastructure.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Company.ProjectName.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Connection string 'Database' was not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
            .Validate(options => options.SigningKey.Length >= 64, "Jwt:SigningKey must be at least 64 characters.")
            .Validate(options => options.AccessTokenMinutes > 0, "Jwt:AccessTokenMinutes must be positive.")
            .Validate(options => options.RefreshTokenDays > 0, "Jwt:RefreshTokenDays must be positive.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                    ?? throw new InvalidOperationException("Jwt configuration is missing.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}

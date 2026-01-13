using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace RentalServiceAspNet.Services;

public static class JwtConfigurationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? "cdkcdncidkcdiccicsncisnicojjjkjkknjbhvhsoncnokkxmxml";
        var key = Encoding.UTF8.GetBytes(secretKey);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "RentalService",
                    ValidAudience = jwtSettings["Audience"] ?? "RentalService",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromMinutes(2),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                        
                        if (string.IsNullOrEmpty(token))
                        {
                            token = context.Request.Query["token"].FirstOrDefault();
                        }
                        
                        if (string.IsNullOrEmpty(token))
                        {
                            token = context.Request.Cookies["token"];
                        }
                        
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("Ошибка аутентификации: {Message}", context.Exception.Message);
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            logger.LogWarning("JWT токен истек");
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal?.Identity as ClaimsIdentity;
                        if (identity != null)
                        {
                            var roleClaim = identity.FindFirst(ClaimTypes.Role) ?? 
                                           identity.FindFirst("role") ??
                                           identity.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                            
                            if (roleClaim != null && roleClaim.Type != ClaimTypes.Role)
                            {
                                var existingRoleClaims = identity.FindAll(ClaimTypes.Role).ToList();
                                foreach (var claim in existingRoleClaims)
                                {
                                    identity.RemoveClaim(claim);
                                }
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                            }
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("Challenge: {Error} - {ErrorDescription}", context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
                
                options.MapInboundClaims = false;
            });

        return services;
    }
}

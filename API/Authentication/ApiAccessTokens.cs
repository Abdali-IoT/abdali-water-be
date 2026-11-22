using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace API.Authentication;

internal sealed record AccessTokenResponse(string AccessToken, string TokenType, int ExpiresIn);

internal sealed class ApiAccessTokens
{
    private const int LifetimeSeconds = 900;
    private readonly JsonWebTokenHandler _tokenHandler = new();
    private readonly SigningCredentials _signingCredentials;

    public ApiAccessTokens(string issuer, string audience, string base64SigningKey)
    {
        Span<byte> keyBytes = new byte[base64SigningKey.Length];
        if (!Convert.TryFromBase64String(base64SigningKey, keyBytes, out var keyLength) || keyLength < 32)
        {
            throw new InvalidOperationException("Authentication:Jwt:SigningKey must be a Base64-encoded key of at least 32 bytes.");
        }

        var signingKey = new SymmetricSecurityKey(keyBytes[..keyLength].ToArray());
        _signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        ValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public TokenValidationParameters ValidationParameters { get; }

    public AccessTokenResponse Issue(Guid userId)
    {
        var now = DateTime.UtcNow;
        var accessToken = _tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = ValidationParameters.ValidIssuer,
            Audience = ValidationParameters.ValidAudience,
            SigningCredentials = _signingCredentials,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddSeconds(LifetimeSeconds),
            Claims = new Dictionary<string, object> { [JwtRegisteredClaimNames.Sub] = userId.ToString("D") }
        });

        return new AccessTokenResponse(accessToken, "Bearer", LifetimeSeconds);
    }
}
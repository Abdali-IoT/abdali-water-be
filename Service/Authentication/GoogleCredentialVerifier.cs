using Google.Apis.Auth;
using Newtonsoft.Json;

namespace Service.Authentication;

public sealed record GoogleIdentity(string Subject, string? FirstSignInEmail);

public sealed class GoogleCredentialVerifier(string clientId)
{
    private readonly GoogleJsonWebSignature.ValidationSettings _settings = new()
    {
        Audience = [clientId]
    };

    public async Task<GoogleIdentity?> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        if (idToken.Length > 10_000)
        {
            return null;
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, _settings)
                .WaitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidJwtException or FormatException or JsonException or ArgumentException)
        {
            return null;
        }

        if (!payload.AudienceAsList.Contains(clientId, StringComparer.Ordinal)
            || string.IsNullOrWhiteSpace(payload.Subject)
            || payload.Subject.Length > 255)
        {
            return null;
        }

        // Google also verifies third-party addresses, but cannot attest to their current ownership.
        var email = payload.Email;
        var canLinkByEmail = payload.EmailVerified
                             && !string.IsNullOrWhiteSpace(email)
                             && (email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
                                 || !string.IsNullOrWhiteSpace(payload.HostedDomain));

        return new GoogleIdentity(payload.Subject, canLinkByEmail ? email : null);
    }
}
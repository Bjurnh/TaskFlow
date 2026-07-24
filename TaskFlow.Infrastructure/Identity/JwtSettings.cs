namespace TaskFlow.Infrastructure.Identity;

/// <summary>
/// Bound from the "Jwt" section of appsettings.json / environment variables in Step 4.
/// Expected keys: Jwt:Secret, Jwt:Issuer, Jwt:Audience, Jwt:AccessTokenExpiryMinutes.
/// Secret must be at least 32 bytes for HmacSha256 - keep it out of source control
/// (use user-secrets locally, App Service configuration/Key Vault in Azure).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
}

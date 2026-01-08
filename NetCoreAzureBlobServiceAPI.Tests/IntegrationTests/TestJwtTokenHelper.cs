using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NetCoreAzureBlobServiceAPI.Tests.IntegrationTests;

/// <summary>
/// Test-only helper to create JWTs for local/integration tests.
/// Tokens created by this class MUST NOT be used in production.
/// </summary>
public static class TestJwtTokenHelper
{
    // Test signing key - intentionally embedded in test code only.
    // Keep this value private to tests and do not add to production config.
    public const string TestSigningKey = "test_signing_key_for_local_dev_please_do_not_use_in_production!";

    // Default audience used by test host
    public const string TestAudience = "api://test";

    public static string GenerateToken(string oid, string? audience = null, string? scope = null, int validMinutes = 60)
    {
        audience ??= TestAudience;
        scope ??= "Files.Manage";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("oid", oid),
            new Claim("scp", scope),
            // Add common alternative claim types to match middleware mappings
            new Claim("scope", scope),
            new Claim("http://schemas.microsoft.com/identity/claims/scope", scope)
        };

        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(validMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}

using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Registration always creates a NEW organization, and the registering user becomes
        // its Owner. No "join an existing org" flow exists yet - that would need an
        // invite-token feature, deliberately out of scope for now.
        var slug = await GenerateUniqueSlugAsync(request.OrganizationName, cancellationToken);
        var organization = Organization.Create(request.OrganizationName, slug);
        _context.Organizations.Add(organization);

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(organization.Id, request.Email, passwordHash, request.DisplayName, UserRole.Owner);
        _context.Users.Add(user);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        // Store only a hash of the refresh token - see RefreshToken entity's doc comment.
        var refreshToken = RefreshToken.Create(user.Id, HashToken(refreshTokenValue), DateTime.UtcNow.AddDays(30));
        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(
            user.Id,
            organization.Id,
            user.Email,
            user.DisplayName,
            accessToken,
            refreshTokenValue);
    }

    private async Task<string> GenerateUniqueSlugAsync(string organizationName, CancellationToken cancellationToken)
    {
        var baseSlug = Slugify(organizationName);
        var slug = baseSlug;
        var suffix = 1;

        while (await _context.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private static string Slugify(string value)
    {
        var lowered = value.Trim().ToLowerInvariant();
        var slug = new string(lowered.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }

    // SHA-256, not BCrypt - the refresh token is already a 64-byte cryptographically random
    // value, not a low-entropy human password, so it doesn't need BCrypt's slow, salted
    // hashing. A fast, deterministic hash is fine (and required, since we look it up by
    // hash equality on refresh, not by verifying against every stored hash).
    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}

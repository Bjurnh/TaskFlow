using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    // "org_id" is a custom claim added when the JWT is issued (see JwtTokenService) -
    // this is what makes multi-tenancy enforcement work everywhere else in the app.
    public Guid? OrganizationId =>
        Guid.TryParse(User?.FindFirstValue("org_id"), out var id) ? id : null;

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
}

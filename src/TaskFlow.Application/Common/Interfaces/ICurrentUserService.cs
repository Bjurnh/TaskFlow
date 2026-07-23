namespace TaskFlow.Application.Common.Interfaces;

/// <summary>
/// Abstracts "who is making this request" away from HttpContext, which Application
/// must never reference. Implemented in the API layer by reading claims off the
/// authenticated JWT (see Step 4). This is also the enforcement point for multi-tenancy:
/// every handler filters by OrganizationId sourced from here, never from client input.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? OrganizationId { get; }
    string? Email { get; }
}

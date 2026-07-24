using MediatR;
using TaskFlow.Application.Features.Auth;

namespace TaskFlow.Application.Features.Auth.Commands.Register;

// Registration creates a NEW Organization (this user becomes its Owner) - this app has no
// "join an existing org" flow yet (that would need an invite-token feature). Simple,
// deliberate scope for a portfolio project: one signup = one new tenant.
public record RegisterCommand(
    string OrganizationName,
    string Email,
    string Password,
    string DisplayName) : IRequest<AuthResultDto>;

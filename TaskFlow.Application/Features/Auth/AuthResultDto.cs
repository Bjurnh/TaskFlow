namespace TaskFlow.Application.Features.Auth;

public record AuthResultDto(
    Guid UserId,
    Guid OrganizationId,
    string Email,
    string DisplayName,
    string AccessToken,
    string RefreshToken);

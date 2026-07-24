namespace TaskFlow.Application.Common.Exceptions;

/// <summary>
/// Distinct from ForbiddenAccessException (403 - "you ARE who you say, but can't do this")
/// vs this one (401 - "we don't believe you are who you say"). Deliberately uses one generic
/// message for both "no such user" and "wrong password" - see LoginCommandHandler.
/// </summary>
public class AuthenticationException : Exception
{
    public AuthenticationException() : base("Invalid email or password.")
    {
    }
}

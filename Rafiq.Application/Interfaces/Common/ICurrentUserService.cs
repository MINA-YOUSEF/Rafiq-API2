namespace Rafiq.Application.Interfaces.Common;

public interface ICurrentUserService
{
    int? UserId { get; }
    string Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

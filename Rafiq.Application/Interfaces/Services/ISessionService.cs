using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Sessions;

namespace Rafiq.Application.Interfaces.Services;

public interface ISessionService
{
    Task<SessionDto> StartSessionAsync(StartSessionRequestDto request, CancellationToken cancellationToken = default);
    Task<SessionDto> SubmitSessionVideoAsync(int sessionId, SubmitSessionVideoRequestDto request, CancellationToken cancellationToken = default);
    Task<SessionDto> GetByIdAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<PagedResult<SessionDto>> GetByChildAsync(int childId, PagedRequest request, CancellationToken cancellationToken = default);
}

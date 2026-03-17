using Rafiq.Application.DTOs.Media;

namespace Rafiq.Application.Interfaces.Services;

public interface IProfileImageService
{
    Task<ProfileImageDto> GetParentProfileImageAsync(CancellationToken cancellationToken = default);
    Task SetParentProfileImageAsync(int mediaId, CancellationToken cancellationToken = default);
    Task RemoveParentProfileImageAsync(CancellationToken cancellationToken = default);

    Task<ProfileImageDto> GetSpecialistProfileImageAsync(CancellationToken cancellationToken = default);
    Task SetSpecialistProfileImageAsync(int mediaId, CancellationToken cancellationToken = default);
    Task RemoveSpecialistProfileImageAsync(CancellationToken cancellationToken = default);

    Task<ProfileImageDto> GetChildProfileImageAsync(int childId, CancellationToken cancellationToken = default);
    Task SetChildProfileImageAsync(int childId, int mediaId, CancellationToken cancellationToken = default);
    Task RemoveChildProfileImageAsync(int childId, CancellationToken cancellationToken = default);
}

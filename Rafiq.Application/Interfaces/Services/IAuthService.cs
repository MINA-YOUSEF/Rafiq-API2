using Rafiq.Application.DTOs.Auth;

namespace Rafiq.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterParentAsync(RegisterParentRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterSpecialistAsync(RegisterSpecialistRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(ChangePasswordRequestDto request, int userId, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task ForceResetAsync(int userId, CancellationToken cancellationToken = default);
}

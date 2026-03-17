using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rafiq.Application.DTOs.Auth;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.External;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Identity;
using Rafiq.Infrastructure.Options;

namespace Rafiq.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;
    private readonly FrontendOptions _frontendOptions;

    public AuthService(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions,
        IOptions<FrontendOptions> frontendOptions)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
        _frontendOptions = frontendOptions.Value;
    }

    public async Task<AuthResponseDto> RegisterParentAsync(RegisterParentRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new BadRequestException("Email is already registered.");
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            IsActive = true,
            EmailConfirmed = true,
            MustChangePassword = false,
            PasswordLastChangedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await _userManager.AddToRoleAsync(user, RoleNames.Parent);

        await _unitOfWork.ParentProfiles.AddAsync(new ParentProfile
        {
            UserId = user.Id,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address
        }, cancellationToken);

        return await BuildAuthResponseAsync(user, [RoleNames.Parent], cancellationToken);
    }

    public async Task<AuthResponseDto> RegisterSpecialistAsync(RegisterSpecialistRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new BadRequestException("Email is already registered.");
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            IsActive = true,
            EmailConfirmed = true,
            MustChangePassword = false,
            PasswordLastChangedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await _userManager.AddToRoleAsync(user, RoleNames.Specialist);

        await _unitOfWork.SpecialistProfiles.AddAsync(new SpecialistProfile
        {
            UserId = user.Id,
            FullName = request.FullName,
            Specialization = request.Specialization,
            Bio = request.Bio
        }, cancellationToken);

        return await BuildAuthResponseAsync(user, [RoleNames.Specialist], cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = _userManager.NormalizeEmail(request.Email);
        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("This account has been deactivated.");
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (user.MustChangePassword)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToArray(),
                RequiresPasswordChange = true
            };
        }

        return await BuildAuthResponseAsync(user, null, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var storedToken = await _unitOfWork.RefreshTokens.Query()
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && !x.IsRevoked, cancellationToken);

        if (storedToken is null || storedToken.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token owner.");
        }

        storedToken.IsRevoked = true;
        storedToken.RevokedAtUtc = DateTime.UtcNow;
        _unitOfWork.RefreshTokens.Update(storedToken);

        return await BuildAuthResponseAsync(user, null, cancellationToken);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequestDto request, int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var stampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!stampResult.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", stampResult.Errors.Select(e => e.Description)));
        }

        user.PasswordLastChangedAt = DateTime.UtcNow;
        user.MustChangePassword = false;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", updateResult.Errors.Select(e => e.Description)));
        }

        await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        var email = user.Email;
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var template = _frontendOptions.ResetPasswordTemplate;

        var resetLink = template
            .Replace("{token}", encodedToken)
            .Replace("{email}", Uri.EscapeDataString(email));

        if (resetLink.Contains('{') || resetLink.Contains('}'))
        {
            throw new InvalidOperationException(
                "ResetPasswordTemplate contains unresolved placeholders.");
        }

        await _emailService.SendAsync(email, "Reset Password", $"Click here: {resetLink}", cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new BadRequestException("Invalid reset request.");

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch (FormatException)
        {
            throw new BadRequestException("Invalid reset request.");
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var stampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!stampResult.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", stampResult.Errors.Select(e => e.Description)));
        }

        user.PasswordLastChangedAt = DateTime.UtcNow;
        user.MustChangePassword = false;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", updateResult.Errors.Select(e => e.Description)));
        }

        await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
    }

    public async Task ForceResetAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found.");

        user.MustChangePassword = true;

        var stampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!stampResult.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", stampResult.Errors.Select(e => e.Description)));
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(
        AppUser user,
        IReadOnlyCollection<string>? explicitRoles,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<string> roles = explicitRoles ?? (await _userManager.GetRolesAsync(user)).ToArray();

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email ?? string.Empty,
            roles,
            user.SecurityStamp ?? string.Empty);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = roles,
            RequiresPasswordChange = false,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = _jwtTokenService.GetAccessTokenExpiryUtc()
        };
    }

    private async Task RevokeAllRefreshTokensAsync(int userId, CancellationToken cancellationToken)
    {
        var tokens = await _unitOfWork.RefreshTokens.Query()
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAtUtc = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}

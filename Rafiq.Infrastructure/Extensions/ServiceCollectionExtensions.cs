using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.External;
using Rafiq.Application.Interfaces.Jobs;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Infrastructure.Data;
using Rafiq.Infrastructure.Identity;
using Rafiq.Infrastructure.Jobs;
using Rafiq.Infrastructure.Options;
using Rafiq.Infrastructure.Repositories;
using Rafiq.Infrastructure.Services;

namespace Rafiq.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.SectionName));
        services.Configure<AiServiceOptions>(configuration.GetSection(AiServiceOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<FrontendOptions>(configuration.GetSection(FrontendOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                }));

        services.AddIdentity<AppUser, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISessionAnalysisJob, SessionAnalysisJob>();
        services.AddScoped<ExerciseReferenceExtractionJob>();

        var aiOptions = configuration.GetSection(AiServiceOptions.SectionName).Get<AiServiceOptions>() ?? new AiServiceOptions();
        var baseUrl = string.IsNullOrWhiteSpace(aiOptions.BaseUrl) ? "http://localhost:8080" : aiOptions.BaseUrl;
        var timeoutSeconds = aiOptions.TimeoutSeconds <= 0 ? 30 : aiOptions.TimeoutSeconds;

        services.AddHttpClient<IAiAnalysisClient, AiAnalysisClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IParentDashboardService, ParentDashboardService>();
        services.AddScoped<ISpecialistDashboardService, SpecialistDashboardService>();
        services.AddScoped<IChildService, ChildService>();
        services.AddScoped<IMedicalReportService, MedicalReportService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<ITreatmentPlanService, TreatmentPlanService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IProgressReportService, ProgressReportService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IProfileImageService, ProfileImageService>();

        return services;
    }
}

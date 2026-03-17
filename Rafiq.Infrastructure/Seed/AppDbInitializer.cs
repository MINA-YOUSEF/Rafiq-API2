using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rafiq.Infrastructure.Data;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Seed;

public static class AppDbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AppDbInitializer");
        var dbContext = services.GetRequiredService<AppDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        var roles = new[] { RoleNames.Admin, RoleNames.Specialist, RoleNames.Parent };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<int>(role));
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to create role {Role}: {Errors}", role, string.Join("; ", result.Errors.Select(x => x.Description)));
                }
            }
        }

        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@rafiq.local";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Admin@12345";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                logger.LogError(
                    "Failed to seed default admin user: {Errors}",
                    string.Join("; ", createResult.Errors.Select(x => x.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, RoleNames.Admin))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
            if (!roleResult.Succeeded)
            {
                logger.LogError(
                    "Failed to assign Admin role to seeded admin user: {Errors}",
                    string.Join("; ", roleResult.Errors.Select(x => x.Description)));
            }
        }
    }
}

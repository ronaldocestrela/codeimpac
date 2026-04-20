using CodeImpact.Infrastructure.Persistence;
using CodeImpact.Domain.Common;
using CodeImpact.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeImpact.WebApi.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigration");

        var dbContext = scope.ServiceProvider.GetRequiredService<CodeImpactDbContext>();
        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

        if (pendingMigrations.Count == 0)
        {
            logger.LogInformation("No pending database migrations were found.");
            await EnsureApplicationRolesAsync(scope.ServiceProvider, logger);
            await EnsureDefaultPlansAsync(scope.ServiceProvider, logger);
            return;
        }

        logger.LogInformation("Applying {Count} pending database migrations.", pendingMigrations.Count);
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        await EnsureApplicationRolesAsync(scope.ServiceProvider, logger);
        await EnsureDefaultPlansAsync(scope.ServiceProvider, logger);
    }

    private static async Task EnsureApplicationRolesAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var roleName in ApplicationRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                logger.LogWarning("Failed to create role {RoleName}: {Errors}", roleName, errors);
                continue;
            }

            logger.LogInformation("Role {RoleName} created successfully.", roleName);
        }
    }

    private static async Task EnsureDefaultPlansAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        var dbContext = serviceProvider.GetRequiredService<CodeImpactDbContext>();
        if (await dbContext.Plans.AnyAsync())
        {
            return;
        }

        var plans = new[]
        {
            new Plan("free", "Plano gratuito para uso individual", 3, 5, 30),
            new Plan("pro", "Plano profissional para usuários frequentes", 20, 50, 90),
            new Plan("premium", "Plano premium com limites expandidos", 100, 200, 365)
        };

        dbContext.Plans.AddRange(plans);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Default plans created successfully.");
    }
}

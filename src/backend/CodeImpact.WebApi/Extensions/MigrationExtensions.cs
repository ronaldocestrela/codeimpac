using CodeImpact.Infrastructure.Persistence;
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
            return;
        }

        logger.LogInformation("Applying {Count} pending database migrations.", pendingMigrations.Count);
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
}

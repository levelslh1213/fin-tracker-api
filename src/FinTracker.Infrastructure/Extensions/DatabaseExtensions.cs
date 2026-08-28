using FinTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinTracker.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<FinTrackerDbContext>>();

        try
        {
            var context = services.GetRequiredService<FinTrackerDbContext>();
            if (context.Database.IsRelational())
            {
                logger.LogInformation("Verificando e aplicando migrations pendentes no PostgreSQL...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations aplicadas com sucesso no PostgreSQL!");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("N?o foi poss?vel conectar ao PostgreSQL para migra??o autom?tica: {Message}", ex.Message);
        }
    }
}

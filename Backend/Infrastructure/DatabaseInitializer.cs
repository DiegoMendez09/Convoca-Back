using Backend.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetService<SecopDbContext>();
        if (context is null)
            return;

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseInitializer");

        try
        {
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("La base de favoritos y borradores está lista.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "No se pudo preparar la base de Azure. Revisa ConnectionStrings:SecopDb. La consulta de SECOP sigue disponible.");
        }
    }
}

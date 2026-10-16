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
            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'dbo.ProfessionalProfiles', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.ProfessionalProfiles
                    (
                        ProfileId int NOT NULL CONSTRAINT PK_ProfessionalProfiles PRIMARY KEY,
                        Title nvarchar(200) NOT NULL,
                        Experience nvarchar(2000) NOT NULL,
                        Keywords nvarchar(300) NOT NULL,
                        UseForSearch bit NOT NULL,
                        UpdatedOn datetime2 NOT NULL
                    );
                END
                """);
            logger.LogInformation("La base de favoritos y perfil está lista.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "No se pudo preparar la base de Azure. Revisa ConnectionStrings:SecopDb. La consulta de SECOP sigue disponible.");
        }
    }
}

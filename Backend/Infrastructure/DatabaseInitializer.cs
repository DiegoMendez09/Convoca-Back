using Backend.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetService<ConvocaDbContext>();
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
            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'dbo.MeritExams', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.MeritExams
                    (
                        ExamId uniqueidentifier NOT NULL CONSTRAINT PK_MeritExams PRIMARY KEY,
                        Title nvarchar(240) NOT NULL,
                        Level nvarchar(40) NOT NULL,
                        OpecCode nvarchar(40) NOT NULL,
                        CallNumber nvarchar(80) NOT NULL,
                        JobTitle nvarchar(200) NOT NULL,
                        QuestionsJson nvarchar(max) NOT NULL,
                        CreatedOn datetime2 NOT NULL
                    );
                END
                """);
            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'dbo.FunctionManuals', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.FunctionManuals
                    (
                        ManualId uniqueidentifier NOT NULL CONSTRAINT PK_FunctionManuals PRIMARY KEY,
                        OriginalFileName nvarchar(260) NOT NULL,
                        OpecCode nvarchar(40) NOT NULL,
                        JobTitle nvarchar(200) NOT NULL,
                        Level nvarchar(40) NOT NULL,
                        Grade nvarchar(20) NOT NULL,
                        Purpose nvarchar(max) NOT NULL,
                        Functions nvarchar(max) NOT NULL,
                        Knowledge nvarchar(max) NOT NULL,
                        SectionText nvarchar(max) NOT NULL,
                        UploadedOn datetime2 NOT NULL
                    );
                END
                """);
            logger.LogInformation("La base de favoritos, perfil, exámenes y manuales está lista.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "convocaApp no pudo preparar la base de Azure. Revisa ConnectionStrings:ConvocaDb. La búsqueda de procesos sigue disponible.");
        }
    }
}

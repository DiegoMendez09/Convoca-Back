-- Ejecutar conectado a la base de Azure que ya existe.
-- La cadena de conexión de la API apunta a esa base en ConnectionStrings:SecopDb.

IF OBJECT_ID(N'dbo.Favorites', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Favorites
    (
        FavoriteId uniqueidentifier NOT NULL CONSTRAINT PK_Favorites PRIMARY KEY,
        ProcessId nvarchar(80) NOT NULL,
        ProcedureName nvarchar(500) NOT NULL,
        EntityName nvarchar(300) NOT NULL,
        Status nvarchar(80) NULL,
        Modality nvarchar(200) NULL,
        BasePrice decimal(18, 2) NULL,
        PublishedOn datetime2 NULL,
        Notes nvarchar(500) NULL,
        CreatedOn datetime2 NOT NULL
    );

    CREATE UNIQUE INDEX IX_Favorites_ProcessId ON dbo.Favorites (ProcessId);
END;

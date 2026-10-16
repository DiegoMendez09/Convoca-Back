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

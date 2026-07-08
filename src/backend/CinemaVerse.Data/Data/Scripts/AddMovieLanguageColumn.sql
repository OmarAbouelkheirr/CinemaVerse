-- Fix: Invalid column name 'Language'
-- Run this script on your CinemaVerse database if the AddMovieLanguage migration was not applied.
-- Table: Movies

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Movies') AND name = 'Language'
)
BEGIN
    ALTER TABLE dbo.Movies
    ADD Language nvarchar(100) NOT NULL DEFAULT N'';
END
GO

-- Record migration as applied so "dotnet ef database update" won't try to add the column again
IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260202233944_AddMovieLanguage')
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260202233944_AddMovieLanguage', N'9.0.0');
END
GO

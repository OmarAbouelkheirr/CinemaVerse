-- =============================================
-- CinemaVerse - Clear All Data Script
-- Deletes all rows from application tables (keeps schema + __EFMigrationsHistory)
-- Run against CinemaVerseDB
-- =============================================

USE CinemaVerseDB;
GO

SET NOCOUNT ON;

-- Delete in order: child tables first (respecting foreign keys)

DELETE FROM BookingPayments;
DELETE FROM BookingSeat;
DELETE FROM Tickets;
DELETE FROM Bookings;
DELETE FROM MovieShowTimes;
DELETE FROM Seats;
DELETE FROM Halls;
DELETE FROM Reviews;
DELETE FROM MovieGenres;
DELETE FROM MovieImages;
DELETE FROM MovieCastMembers;
DELETE FROM Movies;
DELETE FROM Genres;
DELETE FROM Users;
DELETE FROM Branches;

SET NOCOUNT OFF;

PRINT 'All application data cleared successfully.';
GO

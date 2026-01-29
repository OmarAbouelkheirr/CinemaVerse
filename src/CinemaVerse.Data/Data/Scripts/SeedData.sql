-- =============================================
-- CinemaVerse - Seed Data Script
-- Run against CinemaVerseDB
-- =============================================

USE CinemaVerseDB;
GO

SET NOCOUNT ON;

-- -----------------------------------------------
-- 1. Branches
-- -----------------------------------------------
SET IDENTITY_INSERT Branches ON;
INSERT INTO Branches (Id, BranchName, BranchLocation) VALUES
(1, 'Nasr City Branch', 'Nasr City - Mustafa El-Nahhas St'),
(2, 'Maadi Branch', 'Maadi - Corniche El-Maadi');
SET IDENTITY_INSERT Branches OFF;

-- -----------------------------------------------
-- 2. Genres
-- -----------------------------------------------
SET IDENTITY_INSERT Genres ON;
INSERT INTO Genres (Id, GenreName) VALUES
(1, 'Action'),
(2, 'Comedy'),
(3, 'Drama'),
(4, 'Horror'),
(5, 'Romance');
SET IDENTITY_INSERT Genres OFF;

-- -----------------------------------------------
-- 3. Movies
-- Status: 1=Active, MovieAgeRating: 0=G, 1=PG, 2=PG13
-- -----------------------------------------------
SET IDENTITY_INSERT Movies ON;
INSERT INTO Movies (Id, MovieName, MovieDescription, MovieDuration, MovieRating, MovieAgeRating, ReleaseDate, TrailerUrl, MoviePoster, Status) VALUES
(1, 'Mission Impossible', 'Action thriller from the Mission Impossible series starring Tom Cruise.', '02:30:00', 8.5, 2, '2024-07-12', 'https://youtube.com/watch?v=xxx', '/posters/mission.jpg', 1),
(2, 'Night Comedy', 'Egyptian comedy starring Mohamed Henedy.', '01:45:00', 7.0, 1, '2024-08-01', 'https://youtube.com/watch?v=yyy', '/posters/comedy.jpg', 1),
(3, 'The Haunted House', 'Egyptian horror film.', '01:55:00', 6.5, 3, '2024-09-15', 'https://youtube.com/watch?v=zzz', '/posters/horror.jpg', 1);
SET IDENTITY_INSERT Movies OFF;

-- -----------------------------------------------
-- 3b. MovieCastMembers
-- RoleType: 0=Actor, 1=Director, 2=Writer, 3=Producer, etc.
-- -----------------------------------------------
SET IDENTITY_INSERT MovieCastMembers ON;
INSERT INTO MovieCastMembers (Id, MovieId, PersonName, ImageUrl, RoleType, CharacterName, DisplayOrder, IsLead) VALUES
(1, 1, 'Tom Cruise', NULL, 0, 'Ethan Hunt', 0, 1),
(2, 1, 'Hayley Atwell', NULL, 0, NULL, 1, 0),
(3, 2, 'Mohamed Henedy', NULL, 0, NULL, 0, 1),
(4, 2, 'Ahmed Helmy', NULL, 0, NULL, 1, 0),
(5, 3, 'Ahmed Mekky', NULL, 0, NULL, 0, 1),
(6, 3, 'Dina El Sherbiny', NULL, 0, NULL, 1, 0);
SET IDENTITY_INSERT MovieCastMembers OFF;

-- -----------------------------------------------
-- 4. Users
-- Gender: 1=Male, 2=Female
-- -----------------------------------------------
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Address, City, DateOfBirth, IsActive, IsEmailConfirmed, Gender) VALUES
(1, 'ahmed@test.com', 'AQAAAAIAAYagAAAAEHashPlaceholder1', 'Ahmed', 'Mohamed', '01001234567', '10 Tahrir St', 'Cairo', '1995-03-20', 1, 1, 1),
(2, 'sara@test.com', 'AQAAAAIAAYagAAAAEHashPlaceholder2', 'Sara', 'Ali', '01009876543', '5 Maadi St', 'Cairo', '1998-07-14', 1, 1, 2),
(3, 'omar@test.com', 'AQAAAAIAAYagAAAAEHashPlaceholder3', 'Omar', 'Hassan', NULL, 'Nasr City', 'Cairo', '1990-01-05', 1, 0, 1);
SET IDENTITY_INSERT Users OFF;

-- -----------------------------------------------
-- 5. Halls
-- BranchId, HallStatus: 1=Available, HallType: 1=2D, 2=3D
-- -----------------------------------------------
SET IDENTITY_INSERT Halls ON;
INSERT INTO Halls (Id, BranchId, HallNumber, Capacity, HallStatus, HallType) VALUES
(1, 1, '1', 50, 1, 1),
(2, 1, '2', 30, 1, 2),
(3, 2, '1', 40, 1, 1);
SET IDENTITY_INSERT Halls OFF;

-- -----------------------------------------------
-- 6. MovieGenres
-- -----------------------------------------------
INSERT INTO MovieGenres (MovieID, GenreID) VALUES
(1, 1), (1, 2),
(2, 2), (2, 3),
(3, 4), (3, 3);

-- -----------------------------------------------
-- 7. MovieImages
-- -----------------------------------------------
SET IDENTITY_INSERT MovieImages ON;
INSERT INTO MovieImages (Id, ImageUrl, MovieId) VALUES
(1, '/images/m1-1.jpg', 1),
(2, '/images/m1-2.jpg', 1),
(3, '/images/m2-1.jpg', 2),
(4, '/images/m3-1.jpg', 3);
SET IDENTITY_INSERT MovieImages OFF;

-- -----------------------------------------------
-- 8. Reviews
-- -----------------------------------------------
SET IDENTITY_INSERT Reviews ON;
INSERT INTO Reviews (Id, UserId, MovieId, Rating, Comment) VALUES
(1, 1, 1, 9.00, 'Great movie, excellent suspense'),
(2, 2, 1, 8.50, 'Strong action'),
(3, 1, 2, 7.00, 'Light comedy');
SET IDENTITY_INSERT Reviews OFF;

-- -----------------------------------------------
-- 9. Seats
-- Hall 1: A1-A5, B1-B5 (10) -> Id 1-10
-- Hall 2: A1-A4 (4) -> Id 11-14
-- Hall 3: A1-A6 (6) -> Id 15-20
-- -----------------------------------------------
SET IDENTITY_INSERT Seats ON;
INSERT INTO Seats (Id, SeatLabel, HallId) VALUES
(1,'A1',1),(2,'A2',1),(3,'A3',1),(4,'A4',1),(5,'A5',1),
(6,'B1',1),(7,'B2',1),(8,'B3',1),(9,'B4',1),(10,'B5',1),
(11,'A1',2),(12,'A2',2),(13,'A3',2),(14,'A4',2),
(15,'A1',3),(16,'A2',3),(17,'A3',3),(18,'A4',3),(19,'A5',3),(20,'A6',3);
SET IDENTITY_INSERT Seats OFF;

-- -----------------------------------------------
-- 10. MovieShowTimes
-- -----------------------------------------------
SET IDENTITY_INSERT MovieShowTimes ON;
INSERT INTO MovieShowTimes (Id, MovieId, HallId, ShowStartTime, ShowEndTime, Price, CreatedAt) VALUES
(1, 1, 1, DATEADD(DAY, 1, GETUTCDATE()), DATEADD(DAY, 1, DATEADD(MINUTE, 150, GETUTCDATE())), 80.00, GETUTCDATE()),
(2, 2, 1, DATEADD(DAY, 2, GETUTCDATE()), DATEADD(DAY, 2, DATEADD(MINUTE, 105, GETUTCDATE())), 60.00, GETUTCDATE()),
(3, 1, 3, DATEADD(DAY, 1, DATEADD(HOUR, 5, GETUTCDATE())), DATEADD(DAY, 1, DATEADD(MINUTE, 150, DATEADD(HOUR, 5, GETUTCDATE()))), 70.00, GETUTCDATE());
SET IDENTITY_INSERT MovieShowTimes OFF;

-- -----------------------------------------------
-- 11. Bookings
-- Status: 1=Pending, 3=Confirmed
-- -----------------------------------------------
SET IDENTITY_INSERT Bookings ON;
INSERT INTO Bookings (Id, UserId, MovieShowTimeId, Status, TotalAmount) VALUES
(1, 1, 1, 3, 160.00),
(2, 2, 2, 3, 60.00);
SET IDENTITY_INSERT Bookings OFF;

-- -----------------------------------------------
-- 12. BookingPayments
-- Status: 2=Completed
-- -----------------------------------------------
SET IDENTITY_INSERT BookingPayments ON;
INSERT INTO BookingPayments (Id, BookingId, Amount, PaymentIntentId, Currency, Status) VALUES
(1, 1, 160.00, 'pi_seed_001', 'EGP', 2),
(2, 2, 60.00, 'pi_seed_002', 'EGP', 2);
SET IDENTITY_INSERT BookingPayments OFF;

-- -----------------------------------------------
-- 13. BookingSeat
-- -----------------------------------------------
INSERT INTO BookingSeat (BookingId, SeatId) VALUES
(1, 1), (1, 2),
(2, 3);

-- -----------------------------------------------
-- 14. Tickets
-- Status: 1=Active, QrToken and TicketNumber unique
-- -----------------------------------------------
SET IDENTITY_INSERT Tickets ON;
INSERT INTO Tickets (Id, TicketNumber, QrToken, BookingId, SeatId, Price, Status) VALUES
(1, 'TKT000001', 'qr_a1b2c3d4e5f6_001', 1, 1, 80.00, 1),
(2, 'TKT000002', 'qr_a1b2c3d4e5f6_002', 1, 2, 80.00, 1),
(3, 'TKT000003', 'qr_a1b2c3d4e5f6_003', 2, 3, 60.00, 1);
SET IDENTITY_INSERT Tickets OFF;

SET NOCOUNT OFF;

PRINT 'Seed data inserted successfully.';
GO

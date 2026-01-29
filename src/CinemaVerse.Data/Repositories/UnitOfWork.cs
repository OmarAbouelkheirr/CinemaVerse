using System.Data;
using CinemaVerse.Data.Data;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Models.Users;
using CinemaVerse.Data.Repositories.Implementations;
using CinemaVerse.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;


namespace CinemaVerse.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private IBookingRepository? _bookings;
        private IRepository<BookingPayment>? _bookingPayments;
        private IRepository<Branch>? _branches;
        private IRepository<Genre>? _genres;
        private IMovieRepository? _movies;
        private IMovieShowTimeRepository? _movieShowTimes;
        private IRepository<Seat>? _seats;
        private ITicketsRepository? _tickets;
        private IHallRepository? _halls;
        private IRepository<MovieGenre>? _movieGenres;
        private IRepository<MovieImage>? _movieImages;
        private IRepository<MovieCastMember>? _movieCastMembers;
        private IRepository<BookingSeat>? _bookingSeats;
        private IUserRepository? _users;
        private IRepository<Review>? _reviews;

        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly ILoggerFactory _loggerFactory;

        public UnitOfWork(AppDbContext Context,ILoggerFactory loggerFactory)
        {
            _context = Context;
            _loggerFactory = loggerFactory;

        }

        public IBookingRepository Bookings => _bookings ??= new BookingRepository(_context, _loggerFactory.CreateLogger<Booking>());
        public IRepository<BookingPayment> BookingPayments => _bookingPayments ??= new Repository<BookingPayment>(_context, _loggerFactory.CreateLogger<BookingPayment>());
        public IRepository<Branch> Branches => _branches ??= new Repository<Branch>(_context, _loggerFactory.CreateLogger<Branch>());
        public IRepository<Genre> Genres => _genres ??= new Repository<Genre>(_context, _loggerFactory.CreateLogger<Genre>());
        public IMovieRepository Movies => _movies ??= new MovieRepository(_context, _loggerFactory.CreateLogger<Movie>());
        public IMovieShowTimeRepository MovieShowTimes => _movieShowTimes ??= new MovieShowTimeRepository(_context, _loggerFactory.CreateLogger<MovieShowTime>());
        public IRepository<Seat> Seats => _seats ??= new Repository<Seat>(_context, _loggerFactory.CreateLogger<Seat>());
        public ITicketsRepository Tickets => _tickets ??= new TicketsRepository(_context, _loggerFactory.CreateLogger<Ticket>());
        public IHallRepository Halls => _halls ??= new HallRepository(_context, _loggerFactory.CreateLogger<Hall>());
        public IRepository<MovieGenre> MovieGenres => _movieGenres ??= new Repository<MovieGenre>(_context, _loggerFactory.CreateLogger<MovieGenre>());
        public IRepository<MovieImage> MovieImages => _movieImages ??= new Repository<MovieImage>(_context, _loggerFactory.CreateLogger<MovieImage>());
        public IRepository<MovieCastMember> MovieCastMembers => _movieCastMembers ??= new Repository<MovieCastMember>(_context, _loggerFactory.CreateLogger<MovieCastMember>());
        public IRepository<BookingSeat> BookingSeat => _bookingSeats ??= new Repository<BookingSeat>(_context, _loggerFactory.CreateLogger<BookingSeat>());
        public IUserRepository Users => _users ??= new UserRepository(_context, _loggerFactory.CreateLogger<User>());
        public IRepository<Review> Reviews => _reviews ??= new Repository<Review>(_context, _loggerFactory.CreateLogger<Review>());

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            _transaction = await _context.Database.BeginTransactionAsync(isolationLevel);
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction is null)
                return;
            try
            {
                await _transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await RollbackTransactionAsync();
                _loggerFactory.CreateLogger<UnitOfWork>().LogError(ex, "Failed to commit transaction");
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
            await _context.DisposeAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
            _transaction = null;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await  _context.SaveChangesAsync();
        }
    }
}

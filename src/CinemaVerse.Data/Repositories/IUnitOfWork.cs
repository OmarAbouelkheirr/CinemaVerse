using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories.Interfaces;
using System.Data;

namespace CinemaVerse.Data.Repositories
{
    public interface IUnitOfWork : IAsyncDisposable    
    { 
        IBookingRepository Bookings { get; }
        IRepository<BookingPayment> BookingPayments { get; }
        IRepository<Branch> Branches { get; }
        IRepository<Genre> Genres { get; }
        IMovieRepository Movies { get; }
        IMovieShowTimeRepository MovieShowTimes { get; }
        IRepository<Seat> Seats { get; }
        ITicketsRepository Tickets { get; }
        IHallRepository Halls { get; }
        IRepository<MovieGenre> MovieGenres { get; }
        IRepository<MovieImage> MovieImages { get; }
        IRepository<MovieCastMember> MovieCastMembers { get; }
        IRepository<BookingSeat> BookingSeat { get; }
        IUserRepository Users { get; }
        IRepository<Review> Reviews { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task BeginTransactionAsync(IsolationLevel isolationLevel);
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

    }
}

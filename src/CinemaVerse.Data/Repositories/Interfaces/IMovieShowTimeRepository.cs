using CinemaVerse.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IMovieShowTimeRepository : IRepository<MovieShowTime>      
    {
        Task<MovieShowTime?> GetMovieShowTimeWithDetailsAsync(int MovieShowTimeId);
        Task <IEnumerable<Seat>> GetAvailableSeatsAsync(int MovieShowTimeId);
        Task <List<Seat>> GetReservedSeatsAsync(int MovieShowTimeId);
        Task<List<int>> GetReservedSeatIdsAsync(int MovieShowTimeId);

        Task<IEnumerable<Ticket>> GetReservedTicketsAsync(int MovieShowTimeId);
        Task<bool> IsSeatReservedAsync(int movieShowTimeId, int seatId);
    }
}

using CinemaVerse.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface ITicketsRepository : IRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetUserTicketsAsync(Guid UserId);
        Task<Ticket?> GetByTicketNumberAsync(string TicketNumber);
        Task<Ticket?> GetTicketWithDetailsAsync(int TicketId);

    }
}

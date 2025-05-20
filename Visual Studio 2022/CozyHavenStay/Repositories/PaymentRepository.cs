using CozyHavenStay.Contexts;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using Microsoft.EntityFrameworkCore;

namespace CozyHavenStay.Repositories
{
    public class PaymentRepository : Repository<int, Payment>
    {
        public PaymentRepository(CozyHavenStayContext context) : base(context) { }

        public override async Task<IEnumerable<Payment>> GetAll()
        {
            return await _context.Payments
                                 .Include(p => p.Booking)
                                 .ThenInclude(b => b.BookingRooms)   // Include BookingRooms for the relation
                                 .ThenInclude(br => br.Room)          // Then include Room through BookingRoom
                                 .ToListAsync();
        }

        public override async Task<Payment?> GetById(int id)
        {
            return await _context.Payments
                                 .Include(p => p.Booking)
                                 .ThenInclude(b => b.BookingRooms)
                                 .ThenInclude(br => br.Room)
                                 .FirstOrDefaultAsync(p => p.PaymentId == id);
        }
    }
}

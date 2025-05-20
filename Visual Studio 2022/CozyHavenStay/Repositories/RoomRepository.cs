using CozyHavenStay.Contexts;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using Microsoft.EntityFrameworkCore;

namespace CozyHavenStay.Repositories
{
    public class RoomRepository : Repository<int, Room>
    {
        public RoomRepository(CozyHavenStayContext context) : base(context) { }

        public override async Task<IEnumerable<Room>> GetAll()
        {
            return await _context.Rooms
                                 .Include(r => r.Hotel)         // Include the Hotel details
                                 .Include(r => r.BookingRooms)  // Include the BookingRooms (which is the join table)
                                 .ThenInclude(br => br.Booking) // Include the associated Booking through the join table
                                 .ToListAsync();
        }

        public override async Task<Room?> GetById(int id)
        {
            return await _context.Rooms
                                 .Include(r => r.Hotel)
                                 .Include(r => r.BookingRooms)
                                 .ThenInclude(br => br.Booking)
                                 .FirstOrDefaultAsync(r => r.RoomId == id);
        }
    }
}

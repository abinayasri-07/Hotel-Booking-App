using CozyHavenStay.Contexts;
using CozyHavenStay.Models;
using Microsoft.EntityFrameworkCore;

namespace CozyHavenStay.Repositories
{
    public class BookingRoomRepository : Repository<int, BookingRoom>
    {
        public BookingRoomRepository(CozyHavenStayContext context) : base(context)
        {
        }

        public async Task<BookingRoom> Add(BookingRoom entity)
        {
            var result = await _context.BookingRooms.AddAsync(entity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public override async Task<IEnumerable<BookingRoom>> GetAll()
        {
            return await _context.BookingRooms
                                 .Include(br => br.Booking)
                                 .Include(br => br.Room)
                                 .ToListAsync();
        }

        public override async Task<BookingRoom?> GetById(int id)
        {
            return await _context.BookingRooms
                                 .Include(br => br.Booking)
                                 .Include(br => br.Room)
                                 .FirstOrDefaultAsync(br => br.BookingRoomId == id);
        }

        public async Task<BookingRoom> Delete(int id)
        {
            var entity = await GetById(id);
            if (entity == null) throw new Exception($"BookingRoom with ID {id} not found");

            _context.BookingRooms.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<BookingRoom> Update(BookingRoom entity)
        {
            var existing = await GetById(entity.BookingRoomId);
            if (existing == null) throw new Exception($"BookingRoom with ID {entity.BookingRoomId} not found");

            _context.Entry(existing).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existing;
        }
    }
}

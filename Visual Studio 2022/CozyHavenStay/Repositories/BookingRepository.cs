using CozyHavenStay.Contexts;
using CozyHavenStay.Models;
using CozyHavenStay.Repositories;
using Microsoft.EntityFrameworkCore;

public class BookingRepository : Repository<int, Booking>
{
    public BookingRepository(CozyHavenStayContext context) : base(context) { }

    public override async Task<IEnumerable<Booking>> GetAll()
    {
        return await _context.Bookings
            .Include(b => b.BookingRooms) // Include BookingRooms
                .ThenInclude(br => br.Room) // Include the related Room
                    .ThenInclude(r => r.Hotel) // Include the related Hotel for each Room
            .Include(b => b.Customer) // Include the related Customer
            .Include(b => b.Payments) // Include the related Payments
            .ToListAsync();
    }

    public override async Task<Booking?> GetById(int id)
    {
        return await _context.Bookings
            .Include(b => b.BookingRooms) // Include BookingRooms
                .ThenInclude(br => br.Room) // Include the related Room
                    .ThenInclude(r => r.Hotel) // Include the related Hotel for each Room
            .Include(b => b.Customer) // Include the related Customer
            .Include(b => b.Payments) // Include the related Payments
            .FirstOrDefaultAsync(b => b.BookingId == id);
    }
}

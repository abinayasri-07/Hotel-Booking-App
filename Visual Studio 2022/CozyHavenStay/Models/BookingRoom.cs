using System.ComponentModel.DataAnnotations;

namespace CozyHavenStay.Models
{
    public class BookingRoom
    {
        [Key]
        public int BookingRoomId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int RoomId { get; set; }

        // Navigation properties
        public Booking? Booking { get; set; }
        public Room? Room { get; set; }
    }
}

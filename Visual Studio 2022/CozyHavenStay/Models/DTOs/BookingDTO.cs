namespace CozyHavenStay.Models.DTOs
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public List<int> RoomIds { get; set; } = new();
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

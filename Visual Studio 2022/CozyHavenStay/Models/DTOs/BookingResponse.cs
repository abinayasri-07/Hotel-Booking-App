namespace CozyHavenStay.Models.DTOs
{
    public class BookingResponse
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public List<int> RoomIds { get; set; } = new();
        public string Type { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

namespace CozyHavenStay.Models.DTOs
{
    public class CreateBookingRequest
    {
        public int CustomerId { get; set; }
        public List<int> RoomIds { get; set; } = new();
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}

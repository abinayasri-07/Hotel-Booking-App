namespace CozyHavenStay.Models.DTOs
{
    public class BookingConflictCheckRequest
    {
        public List<int> RoomIds { get; set; } = new();
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}

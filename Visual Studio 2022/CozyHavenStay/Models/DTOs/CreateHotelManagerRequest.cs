using System.ComponentModel.DataAnnotations;

namespace CozyHavenStay.Models.DTOs
{
    public class CreateHotelManagerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
    public class HotelManagerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? HotelName { get; set; }
        //public int HotelId { get; set; }
        //public string? Location { get; set; }
        //public string? ContactNumber { get; set; }
        ////public string Email { get; set; } = string.Empty;
        //public string? Description { get; set; }
        //public decimal PricePerNight { get; set; }
        //public int StarRating { get; set; }
        //public string? Amenities { get; set; }
        //public string? Url { get; set; }
        //public int RoomId { get; set; }
        //public string? Type { get; set; }
        //public int Capacity { get; set; }
        //public bool IsAvailable { get; set; } = true;
        //public string Amenities { get; set; } = string.Empty; 
        //public string Description { get; set; } = string.Empty;
        //public string Url { get; set; } = string.Empty; 

    }
}

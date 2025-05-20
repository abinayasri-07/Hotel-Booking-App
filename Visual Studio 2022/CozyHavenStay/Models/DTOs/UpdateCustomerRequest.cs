﻿namespace CozyHavenStay.Models.DTOs
{
    public class UpdateCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}

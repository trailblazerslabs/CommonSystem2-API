﻿namespace CommonSystem2_API.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Name { get; set; }
        public string? Organization { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}

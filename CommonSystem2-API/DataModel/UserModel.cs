﻿namespace CommonSystem2_API.DataModel
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Name { get; set; }
        public string? Organization { get; set; }
        public string? Role { get; set; }
    }
}

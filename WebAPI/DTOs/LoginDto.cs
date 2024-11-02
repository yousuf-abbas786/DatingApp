﻿using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class LoginDto
    {
        public required string Username { get; set; }

        public required string Password { get; set; }
    }
}

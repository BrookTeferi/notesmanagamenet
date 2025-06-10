﻿using System.ComponentModel.DataAnnotations;

namespace NotesManagement.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
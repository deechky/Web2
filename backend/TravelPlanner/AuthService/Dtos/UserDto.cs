using System;

namespace AuthService.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Uloga { get; set; } = string.Empty;
        public DateTime DatumKreiranja { get; set; }
    }
}

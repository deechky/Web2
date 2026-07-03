using System;

namespace AuthService.Models
{
    public enum UserRole
    {
        Korisnik,
        Admin
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LozinkaHash { get; set; } = string.Empty;
        public UserRole Uloga { get; set; } = UserRole.Korisnik;
        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;
    }
}

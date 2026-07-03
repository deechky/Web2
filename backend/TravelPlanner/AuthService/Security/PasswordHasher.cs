namespace AuthService.Security
{
    public static class PasswordHasher
    {
        public static string Hash(string lozinka)
        {
            return BCrypt.Net.BCrypt.HashPassword(lozinka);
        }

        public static bool Verify(string lozinka, string lozinkaHash)
        {
            return BCrypt.Net.BCrypt.Verify(lozinka, lozinkaHash);
        }
    }
}

using System.Security.Cryptography;
using System.Text;

namespace LMSApi.BALLibrary.Utils
{
    public static class PasswordHashing
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;

        public static (string Hash, string Salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public static bool VerifyPassword(string password, string storedHash, string? storedSalt)
        {
            if (string.IsNullOrWhiteSpace(storedSalt))
            {
                var legacyHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
                return storedHash == legacyHash;
            }

            var salt = Convert.FromBase64String(storedSalt);
            var expectedHash = Convert.FromBase64String(storedHash);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);

            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
    }
}
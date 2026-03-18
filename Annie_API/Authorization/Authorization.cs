using System.Security.Cryptography;

namespace Annie_API.Authorization
{
    public class Authorizator
    {
        private const int saltLength = 16; // 16 bytes salt
        private const int hashLength = 32;
        private const int iterations = 120000; // Number of iterations for PBKDF2

        public string HashPassword(string password)
        {
            // Implements a salted hash algorithm using PBKDF2 (Password-Based Key Derivation Function 2)
            
            byte[] salt = new byte[saltLength];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, new HashAlgorithmName("SHA256"), hashLength);

            byte[] hashBytes = new byte[saltLength + hashLength];

            Array.Copy(salt, 0, hashBytes, 0, saltLength);
            Array.Copy(hash, 0, hashBytes, saltLength, hashLength);

            string savedPasswordHash = Convert.ToBase64String(hashBytes);

            return savedPasswordHash;

        }

        public bool VerifyPassword(string password, string savedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(savedHash);
            byte[] salt = new byte[saltLength];
            Array.Copy(hashBytes, 0, salt, 0, saltLength);

            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, new HashAlgorithmName("SHA256"), hashLength);

            for (int i = 0; i < hashLength; i++)
                if (hashBytes[i + saltLength] != hash[i])
                    return false;

            return true;
        }

    }
}

using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    // Hash the password using PBKDF2
    public static string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt = new byte[16]; // 16-byte salt
        using (var rng = RandomNumberGenerator.Create()) // Use RandomNumberGenerator
        {
            rng.GetBytes(salt);
        }

        // Use PBKDF2 to hash the password with the salt
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256)) // 10,000 iterations and SHA256 algorithm
        {
            byte[] hash = pbkdf2.GetBytes(32); // 32-byte hash (SHA256)
            byte[] hashBytes = new byte[48]; // 16-byte salt + 32-byte hash

            Array.Copy(salt, 0, hashBytes, 0, 16); // Copy the salt into the final array
            Array.Copy(hash, 0, hashBytes, 16, 32); // Copy the hash into the final array

            return Convert.ToBase64String(hashBytes); // Return the result as a Base64 string
        }
    }

    // Verify if the entered password matches the stored hash
    public static bool VerifyPassword(string enteredPassword, string storedHash)
    {
        // Convert the Base64 string of the stored hash into a byte array
        byte[] hashBytes = Convert.FromBase64String(storedHash);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16); // Extract the salt from the stored hash
        byte[] storedPasswordHash = new byte[32];
        Array.Copy(hashBytes, 16, storedPasswordHash, 0, 32); // Extract the password hash

        // Apply PBKDF2 with the salt to hash the entered password
        using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000, HashAlgorithmName.SHA256)) // 10,000 iterations and SHA256 algorithm
        {
            byte[] enteredPasswordHash = pbkdf2.GetBytes(32); // 32-byte hash (SHA256)
            return enteredPasswordHash.SequenceEqual(storedPasswordHash); // Compare the hashes
        }
    }
}

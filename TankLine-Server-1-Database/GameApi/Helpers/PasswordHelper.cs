using BCrypt.Net;

public static class PasswordHelper
{
    // Hash the password using bcrypt
    public static string HashPassword(string password)
    {
        // Generate a salt with a work factor of 12 (default)
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12); 
        return BCrypt.Net.BCrypt.HashPassword(password, salt); 
    }

    // Verify if the entered password matches the stored hash
    public static bool VerifyPassword(string enteredPassword, string storedHash)
    {
        try
        {
            // Compare the entered password with the stored hash using bcrypt
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Handle salt version issues and re-hash the password if necessary
            // Log the issue or re-hash the password if needed
            return false;
        }
    }
}

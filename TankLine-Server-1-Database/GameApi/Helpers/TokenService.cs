using BCrypt.Net;

public class TokenService
{
    public static string HashRefreshToken(string refreshToken)
    {
        // Combine the refresh token with the username
      
        Console.WriteLine($"Token with username (to hash): {refreshToken}");

        // Hash the combined value
        string hashedToken = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        Console.WriteLine($"Generated hash: {hashedToken}");
        return hashedToken;
    }

    public static bool VerifyRefreshToken(string refreshToken, string storedHashedToken)
    {   

        Console.WriteLine($"Token with username: {refreshToken}");
        Console.WriteLine($"Stored hash: {storedHashedToken}");

        // Verify the combined value against the stored hash
        bool isValid = BCrypt.Net.BCrypt.Verify(refreshToken, storedHashedToken);
        Console.WriteLine($"Verification result: {isValid}");
        return isValid;
    }
}
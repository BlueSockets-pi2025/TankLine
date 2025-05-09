using BCrypt.Net;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Text;

public class TokenService
{
    public static string GenerateRefreshToken(string username)
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] randomBytes = new byte[64]; // 64 bytes for better security
            rng.GetBytes(randomBytes);
            string randomToken = Convert.ToBase64String(randomBytes);

            // Combine the username with the random token
            string tokenWithUsername = $"{username}:{randomToken}"; // Base64-encoded strings never include the ":"
            return tokenWithUsername;
        }
    }

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
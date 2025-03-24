using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using GameApi.Data;
using GameApi.Models;
using System;
using System.Net.Mail;
using System.Net;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations; // Added for in-memory verification code management

[ApiController]
[Route("api/auth")]
public class ConnectionRegistrationController : Controller
{
    private readonly GameDbContext _context;
    private readonly IConfiguration _configuration;

    public ConnectionRegistrationController(GameDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] UserAccount user)
{
    if (!ModelState.IsValid)
    {
        return BadRequest("Invalid data");
    }

    // empty values or values only composed of whitespaces 
    if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName)||
        string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.PasswordHash))
    {
        return BadRequest("The values cannot be empty or contain only whitespace.");
    }

    if (!user.PasswordHash.Any(char.IsDigit) || !user.PasswordHash.Any(char.IsPunctuation))
    {
        return BadRequest("Password must contain at least one numeric character and one special character.");
    }

    if (user.PasswordHash.Length < 8)
    {
        return BadRequest("Password must be at least 8 characters long.");
    }


    var existingUser = _context.UserAccounts.FirstOrDefault(u => u.Username == user.Username || u.Email == user.Email);

    if (existingUser != null)
    {
        if (existingUser.IsVerified)
        {
            return BadRequest("Username or email already exists.");
        }
        else
        {
            // Remove the unverified account
            _context.UserAccounts.Remove(existingUser);
            await _context.SaveChangesAsync();
        }
    }

    try
    {
        string verificationCode = new Random().Next(100000, 999999).ToString();

        // Hash password before saving it
        user.PasswordHash = PasswordHelper.HashPassword(user.PasswordHash);
        user.BirthDate = user.BirthDate.ToUniversalTime();

        // Add verification code directly to UserAccount
        user.VerificationCode = verificationCode;
        user.VerificationExpiration = DateTime.UtcNow.AddMinutes(5); // Code valid for 5 minutes

        // Add the new user to the database
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        Console.WriteLine($"Verification code for {user.Email}: {verificationCode}");

        SendVerificationEmail(user.Email, verificationCode);

        return Ok("Verification code sent to your email.");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}


    [HttpPost("verify")]
    public async Task<IActionResult> VerifyAccount([FromBody] VerificationRequest request)
    {   
        var user = _context.UserAccounts.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            return BadRequest("Invalid user.");
        }

        if (user.IsVerified)
        {
            return BadRequest("Account already verified.");
        }

        // Check if the verification code is correct and not expired
        if (user.VerificationCode != request.Code || user.VerificationExpiration < DateTime.UtcNow)
        {
            return BadRequest("Invalid or expired verification code.");
        }

        // Mark the user as verified
        user.IsVerified = true;
        user.VerificationCode = null; // Remove the code after verification
        user.VerificationExpiration = null;
        user.BirthDate = user.BirthDate.ToUniversalTime();

        user.CreatedAt = user.CreatedAt.ToUniversalTime();

        _context.UserAccounts.Update(user);
        await _context.SaveChangesAsync();

        // Create a default entry in user_statistics
        var userStatistics = new UserStatistic
        {
            Username = user.Username // Use the username from UserAccount
        };

        _context.UserStatistics.Add(userStatistics);
        
        await _context.SaveChangesAsync();

        Console.WriteLine($"Account verified successfully for {request.Email}");

        return Ok("Account verified successfully.");
    }


    private void SendVerificationEmail(string email, string code)
    {
        var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
        {
            Port = int.TryParse(_configuration["EmailSettings:Port"], out var port) ? port : 587,
            Credentials = new NetworkCredential(
                _configuration["EmailSettings:Username"] ?? string.Empty,
                _configuration["EmailSettings:Password"] ?? string.Empty),
            EnableSsl = true
        };

        string htmlContent = $@"
        <html>
            <body style='font-family: Arial, sans-serif; background-color: #f7f9fc; color: #333; padding: 20px;'>
                <div style='text-align: center; padding: 20px;'>
                    <h1 style='color: #007bff; font-size: 32px; font-weight: 600;'>Welcome to TankLine</h1>
                    <p style='font-size: 18px; color: #555;'>You requested a login code to access TankLine, your epic multiplayer game.</p>
                    <p style='font-size: 22px; font-weight: bold; color: #007bff;'>Your login code is:</p>
                    <h2 style='color: #007bff; font-size: 48px; font-weight: bold;'>{code}</h2>
                    <p style='font-size: 16px; color: #555;'>This code is valid for 5 minutes. Please use it quickly.</p>
                    <p style='font-size: 14px; color: #888;'>If you did not request this code, please ignore this email.</p>
                </div>
                <div style='text-align: center; margin-top: 40px; font-size: 14px; color: #888;'>
                    <p>The Blue Socket Team - Creators of TankLine</p>
                    <p>TankLine | <a href='https://www.bluesocket.com' style='color: #007bff;'>bluesocket.com</a></p>
                </div>
            </body>
        </html>";

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:FromEmail"] ?? "default@example.com"),
            Subject = "Your Login Code for TankLine",
            Body = htmlContent,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        try
        {
            smtpClient.Send(mailMessage);
            Console.WriteLine($"Email sent to {email}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }


    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.UsernameOrEmail) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest("Username, email, and password are required.");
        }

        var user = _context.UserAccounts.FirstOrDefault(u =>
            u.Username == loginRequest.UsernameOrEmail ||
            u.Email == loginRequest.UsernameOrEmail);

        if (user == null || !PasswordHelper.VerifyPassword(loginRequest.Password, user.PasswordHash))
        {
            return BadRequest("Incorrect username or password.");
        }

        if (!user.IsVerified)
        {
            return BadRequest("Account not verified. Please check your email.");
        }

        var token = GenerateJwtToken(user);
        Response.Cookies.Append("AuthToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(Convert.ToInt32(_configuration["JwtSettings:ExpiryHours"]))
        });

        return Ok(new { Token = token });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken");
        return Ok("Logged out successfully.");
    }

    [HttpPost("resend-verification-code")]
    public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationRequest request)
    {
        var user = _context.UserAccounts.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        if (user.IsVerified)
        {
            return BadRequest("Account already verified.");
        }

        // Generate a new code
        string newVerificationCode = new Random().Next(100000, 999999).ToString();

        // Update the verification code and expiration directly in UserAccount
        user.VerificationCode = newVerificationCode;
        user.VerificationExpiration = DateTime.UtcNow.AddMinutes(5); // Valid for 5 minutes

        // Save changes in the database
        await _context.SaveChangesAsync();

        Console.WriteLine($"New verification code for {user.Email}: {newVerificationCode}");

        // Resend the verification email
        SendVerificationEmail(user.Email, newVerificationCode);

        return Ok("New verification code sent to your email.");
    }


    private string GenerateJwtToken(UserAccount user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Sub, user.Username ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var secretKey = _configuration["JwtSettings:SecretKey"] ?? string.Empty;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"] ?? string.Empty,
            audience: _configuration["JwtSettings:Audience"] ?? string.Empty,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(Convert.ToInt32(_configuration["JwtSettings:ExpiryHours"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
    {
        var user = _context.UserAccounts.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        
        string resetCode = new Random().Next(100000, 999999).ToString();
        var expirationTime = DateTime.UtcNow.AddMinutes(5); // Expiration in 5 minutes

        // Update user account information with token and expiration date
        user.PasswordResetToken = resetCode;
        user.PasswordResetExpiration = expirationTime;

        if (user.PasswordResetExpiration.HasValue)
        {
            user.PasswordResetExpiration = DateTime.SpecifyKind(user.PasswordResetExpiration.Value, DateTimeKind.Utc);
        }
        user.CreatedAt = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);
        user.BirthDate = DateTime.UtcNow.Date;


        // Save changes to the database
        _context.UserAccounts.Update(user);
        await _context.SaveChangesAsync();

        // Send email with code
        SendPasswordResetEmail(user.Email, resetCode);

        return Ok("Password reset code has been sent to your email.");
    }



    public void SendPasswordResetEmail(string email, string resetCode)
    {
        var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
        {
            Port = int.TryParse(_configuration["EmailSettings:Port"], out var port) ? port : 587,
            Credentials = new NetworkCredential(
                _configuration["EmailSettings:Username"] ?? string.Empty,
                _configuration["EmailSettings:Password"] ?? string.Empty),
            EnableSsl = true
        };

        string htmlContent = $@"
        <html>
            <body style='font-family: Arial, sans-serif; background-color: #f7f9fc; color: #333; padding: 20px;'>
                <div style='text-align: center; padding: 20px;'>
                    <h1 style='color: #007bff; font-size: 32px; font-weight: 600;'>Password Reset Request</h1>
                    <p style='font-size: 18px; color: #555;'>Use the following code to reset your password:</p>
                    <h2 style='color: #007bff; font-size: 48px; font-weight: bold;'>{resetCode}</h2>
                    <p style='font-size: 16px; color: #555;'>This code is valid for 5 minutes.</p>
                    <p style='font-size: 14px; color: #888;'>If you did not request this, please ignore this email.</p>
                </div>
                <div style='text-align: center; margin-top: 40px; font-size: 14px; color: #888;'>
                    <p>The Blue Socket Team - Creators of TankLine</p>
                    <p>TankLine | <a href='https://www.bluesocket.com' style='color: #007bff;'>bluesocket.com</a></p>
                </div>
            </body>
        </html>";

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:FromEmail"] ?? "default@example.com"),
            Subject = "Your Password Reset Code for TankLine",
            Body = htmlContent,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        try
        {
            smtpClient.Send(mailMessage);
            Console.WriteLine($"Password reset code sent to {email}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        Console.WriteLine($"In reset-password");

        // Search user by email
        var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        // Check that the reset code is correct
        if (user.PasswordResetToken == null)
        {
            return BadRequest("No reset code found.");
        }

        if (DateTime.UtcNow > user.PasswordResetExpiration)
        {
            user.PasswordResetToken = null; // Delete expired code
            user.PasswordResetExpiration = null; // Delete expiration
            await _context.SaveChangesAsync();
            return BadRequest("Reset code expired.");
        }

        // Check that the code matches
        if (user.PasswordResetToken != request.Code)
        {
            return BadRequest("Invalid reset code.");
        }

        var passwordRegex = new System.Text.RegularExpressions.Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{8,}$");
        if (!passwordRegex.IsMatch(request.NewPassword))
            {
                return BadRequest(
                    "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character."
                );
            }
        
        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest(
                    "Passwords do not match");
        }
        // Hash the new password and save it
        user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
        
        // Delete code after use
        user.PasswordResetToken = null;
        user.PasswordResetExpiration = null;

        await _context.SaveChangesAsync();

        return Ok("Password reset successfully.");
    }


}

public class PasswordResetRequest
{
    public required string Email { get; set; }
}

public class ResetPasswordRequest
{
    public required string Email { get; set; }
    public required string Code { get; set; } 
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }


}



public class VerificationRequest
{
    public required string Email { get; set; }
    public required string Code { get; set; }
}

public class LoginRequest
{
    public required string UsernameOrEmail { get; set; }
    public required string Password { get; set; }
}

public class ResendVerificationRequest
{
    public required string Email { get; set; }
}

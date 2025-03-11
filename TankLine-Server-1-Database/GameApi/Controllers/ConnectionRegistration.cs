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
using Microsoft.EntityFrameworkCore; // Added for in-memory verification code management

[ApiController]
[Route("api/auth")]
public class ConnectionRegistrationController : Controller
{
    private readonly GameDbContext _context;
    private readonly IConfiguration _configuration;

    // Dictionary to temporarily store verification codes in memory
    private static Dictionary<string, (string Code, DateTime Expiration)> verificationCodes = new();


    public ConnectionRegistrationController(GameDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid data.");
        }

        if (_context.UserAccounts.Any(u => u.Username == request.Username || u.Email == request.Email))
        {
            return BadRequest("Username or email already exists.");
        }

        // Verifying first name and last name
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest("First name and last name are required.");
        }

        if (request.BirthDate == default)
        {
            return BadRequest("Invalid birth date.");
        }

        // Verifying minimum age (CONSIDERED 13 )
        var minBirthDate = DateTime.UtcNow.AddYears(-13);
        if (request.BirthDate > minBirthDate)
        {
            return BadRequest("You must be at least 13 years old to register.");
        }

        // Verifying the correspondance between Password and ConfirmPassword
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest("Password and Confirm Password do not match.");
        }

        try
        {
            string verificationCode = new Random().Next(100000, 999999).ToString();

            var user = new UserAccount
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                PasswordHash = PasswordHelper.HashPassword(request.Password)
            };

            _context.UserAccounts.Add(user);
            user.BirthDate = DateTime.SpecifyKind(user.BirthDate, DateTimeKind.Utc);

            await _context.SaveChangesAsync();

            // Creating the entry in the VerificationCode table
            var verificationEntry = new VerificationCode
            {
                Email = user.Email,
                Code = verificationCode,
                Expiration = DateTime.UtcNow.AddMinutes(10),
            };

            _context.VerificationCodes.Add(verificationEntry);
            await _context.SaveChangesAsync();

            //sending the verification code by email 
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

        var verificationEntry = await _context.VerificationCodes
            .FirstOrDefaultAsync(v => v.Email == request.Email && v.Code == request.Code);

        if (verificationEntry == null || verificationEntry.Expiration < DateTime.UtcNow)
        {
            return BadRequest("Invalid or expired verification code.");
        }

        user.CreatedAt = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);

        user.IsVerified = true;
        _context.UserAccounts.Update(user);
        _context.VerificationCodes.Remove(verificationEntry);
        await _context.SaveChangesAsync();

        Console.WriteLine($"Account verified successfully for {request.Email}");

        return Ok("Account verified successfully.");
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
        
        // Update code and expiration date
        verificationCodes[user.Email] = (newVerificationCode, DateTime.UtcNow.AddMinutes(5)); 

        Console.WriteLine($"New verification code for {user.Email}: {newVerificationCode}");

        // Resend email
        SendVerificationEmail(user.Email, newVerificationCode);

        return Ok("New verification code sent to your email.");
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

        // Générer un code de 6 chiffres
        string resetCode = new Random().Next(100000, 999999).ToString();
        var expirationTime = DateTime.UtcNow.AddMinutes(5); // Expiration en 5 minutes

        // Stocker le code en mémoire
        verificationCodes[user.Email] = (resetCode, expirationTime);

        // Envoyer l'email avec le code
        SendPasswordResetEmail(user.Email, resetCode);

        return Ok("Password reset code has been sent to your email.");
    }


    private void SendPasswordResetEmail(string email, string resetCode)
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

        var user = _context.UserAccounts.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        if (!verificationCodes.ContainsKey(request.Email))
        {
            return BadRequest("No reset code found.");
        }

        var (storedCode, expiration) = verificationCodes[request.Email];

        if (DateTime.UtcNow > expiration)
        {
            verificationCodes.Remove(request.Email);
            return BadRequest("Reset code expired.");
        }

        if (storedCode != request.Code)
        {
            return BadRequest("Invalid reset code.");
        }

        // Hacher le nouveau mot de passe et l'enregistrer
        user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
        _context.UserAccounts.Update(user);
        
        user.CreatedAt = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);

        await _context.SaveChangesAsync();

        // Supprimer le code après utilisation
        verificationCodes.Remove(request.Email);

        return Ok("Password reset successfully.");
    }

}

public class RegisterRequest
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required DateTime BirthDate { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
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

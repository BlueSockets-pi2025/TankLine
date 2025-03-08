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
using System.Collections.Generic; // Added for in-memory verification code management

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

    // Register a new user
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserAccount user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid data");
        }

        if (_context.UserAccounts.Any(u => u.Username == user.Username || u.Email == user.Email))
        {
            return BadRequest("Username or email already exists.");
        }

        try
        {
            string verificationCode = new Random().Next(100000, 999999).ToString();
            
            // Hash password before saving it
            user.PasswordHash = PasswordHelper.HashPassword(user.PasswordHash);

            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();

            verificationCodes[user.Email] = (verificationCode, DateTime.UtcNow.AddMinutes(1)); 

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
        // Search for the user in the database
        var user = _context.UserAccounts.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            return BadRequest("Invalid user.");
        }
        if (!verificationCodes.ContainsKey(request.Email))
        {
            return BadRequest("No verification code.");
        }

        // Retrieve code and expiration time
        var (code, expiration) = verificationCodes[request.Email];

        if (DateTime.UtcNow > expiration)
        {
            verificationCodes.Remove(request.Email); // Delete expired code
            return BadRequest("Verification code expired.");
        }

        if (code != request.Code)
        {
            return BadRequest("Invalid verification code.");
        }

        // Account verified, update user in memory (not in DB)
        user.IsVerified = true; // Do not persist, just change in memory for the session
        _context.UserAccounts.Update(user);

        user.CreatedAt = DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);

        await _context.SaveChangesAsync();

        // Delete verification code after validation
        verificationCodes.Remove(request.Email);

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

    string resetToken = Guid.NewGuid().ToString();
    var expirationTime = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour

    // Store the token and expiration in memory or database
    verificationCodes[user.Email] = (resetToken, expirationTime);

    // Send password reset email with the token
    SendPasswordResetEmail(user.Email, resetToken);

    return Ok("Password reset link has been sent to your email.");
}

private void SendPasswordResetEmail(string email, string resetToken)
{
    var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
    {
        Port = int.TryParse(_configuration["EmailSettings:Port"], out var port) ? port : 587,
        Credentials = new NetworkCredential(
            _configuration["EmailSettings:Username"] ?? string.Empty,
            _configuration["EmailSettings:Password"] ?? string.Empty),
        EnableSsl = true
    };

    string resetLink = $"{_configuration["AppSettings:BaseUrl"]}/reset-password?token={resetToken}";
    
    string htmlContent = $@"
    <html>
        <body style='font-family: Arial, sans-serif; background-color: #f7f9fc; color: #333; padding: 20px;'>
            <div style='text-align: center; padding: 20px;'>
                <h1 style='color: #007bff; font-size: 32px; font-weight: 600;'>Password Reset Request</h1>
                <p style='font-size: 18px; color: #555;'>You requested a password reset for your account on TankLine.</p>
                <p style='font-size: 16px; color: #555;'>Click the link below to reset your password:</p>
                <a href='{resetLink}' style='font-size: 18px; font-weight: bold; color: #007bff;'>Reset Password</a>
                <p style='font-size: 14px; color: #888;'>This link will expire in 1 hour.</p>
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
        Subject = "Password Reset Request for TankLine",
        Body = htmlContent,
        IsBodyHtml = true
    };
    mailMessage.To.Add(email);

    try
    {
        smtpClient.Send(mailMessage);
        Console.WriteLine($"Password reset email sent to {email}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending email: {ex.Message}");
    }
}


[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
{
    if (!verificationCodes.ContainsKey(request.Email) || verificationCodes[request.Email].Expiration < DateTime.UtcNow)
    {
        return BadRequest("Invalid or expired reset token.");
    }

    if (verificationCodes[request.Email].Code != request.Token)
    {
        return BadRequest("Invalid reset token.");
    }

    var user = _context.UserAccounts.FirstOrDefault(u => u.Email == request.Email);
    if (user == null)
    {
        return BadRequest("User not found.");
    }

    // Hash the new password and update it
    user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);

    _context.UserAccounts.Update(user);
    await _context.SaveChangesAsync();

    // Remove the token after it's used
    verificationCodes.Remove(request.Email);

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
    public required string Token { get; set; }
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

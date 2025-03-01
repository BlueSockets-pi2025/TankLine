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

    // Register a new user
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserAccount user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid data");
        }

        // Check if the username or email already exists
        if (_context.UserAccounts.Any(u => u.Username == user.Username || u.Email == user.Email))
        {
            return BadRequest("Username or email already exists.");
        }

        try
        {
            // Hash the password
            string hashedPassword = PasswordHelper.HashPassword(user.PasswordHash);

            // Create a new user
            user.PasswordHash = hashedPassword;

            // Add the user to the database
            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();

            // Return a success response (Code 200)
            return Ok("Registration successful");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // Login an existing user and generate a JWT token
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.UsernameOrEmail) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest("Username, email, and password are required.");
        }

        var user = _context.UserAccounts.FirstOrDefault(u =>
            u.Username == loginRequest.UsernameOrEmail ||
            u.Email == loginRequest.UsernameOrEmail); // Check if the input is a username or an email

        if (user == null || !PasswordHelper.VerifyPassword(loginRequest.Password, user.PasswordHash))
        {
            return BadRequest("Incorrect username or password.");
        }

        // Generate JWT Token
        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    // Helper method to generate a JWT token
    private string GenerateJwtToken(UserAccount user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username ?? string.Empty), // Ensure that Username is not null
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty), // Ensure that Email is not null
            // Add other claims if necessary, such as roles, permissions, etc.
        };

        var secretKey = _configuration["JwtSettings:SecretKey"] ?? string.Empty; // Provide a default value if null
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)); // Your secret key
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"] ?? string.Empty, // Provide a default value if null
            audience: _configuration["JwtSettings:Audience"] ?? string.Empty, // Provide a default value if null
            claims: claims,
            expires: DateTime.Now.AddHours(Convert.ToInt32(_configuration["JwtSettings:ExpiryHours"])), // Token expires in 1 hour (from config)
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token); // Generate the token as a string
    }

}

// DTO for login (with username or email and password)
public class LoginRequest
{
    public required string UsernameOrEmail { get; set; } // Can be either a username or an email
    public required string Password { get; set; }
}

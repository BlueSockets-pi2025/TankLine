using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GameApi.Data ; 
using GameApi.Models ; 


[ApiController]
[Route("api/player")]
public class UserController : Controller
{
    private readonly GameDbContext _context;

    public UserController(GameDbContext context)
    {
        _context = context;
    }

    // Register a new user
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserAccount user)
    {
        if (ModelState.IsValid)
        {
            // Check if the username or email already exists
            if (_context.UserAccounts.Any(u => u.Username == user.Username || u.Email == user.Email))
            {
                return BadRequest("Username or email already exists.");
            }

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

        return BadRequest("Invalid data");
    }

    // Login an existing user
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        var user = _context.UserAccounts.FirstOrDefault(u => 
            u.Username == loginRequest.UsernameOrEmail || 
            u.Email == loginRequest.UsernameOrEmail); // Checks if input is a username or an email (login is possible with both)

        if (user != null && PasswordHelper.VerifyPassword(loginRequest.Password, user.PasswordHash))
        {
            return Ok("Login successful");
        }
        else
        {
            return BadRequest("Incorrect username or password.");
        }
    }
}

// DTO for login (with username or email and password)
public class LoginRequest
{
    public required string UsernameOrEmail { get; set; } // Can be either a username or an email
    public required string Password { get; set; }
}

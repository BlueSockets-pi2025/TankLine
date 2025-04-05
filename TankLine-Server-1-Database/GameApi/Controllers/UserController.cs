using GameApi.Data;
using GameApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace GameApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly GameDbContext _context;

        public UserController(GameDbContext context)
        {
            _context = context;
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                return Ok("SUCCESS : CONNECTION SUCCESSFUL\n");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"ERROR : FAILED CONNECTION {ex.Message}\n");
            }
        }
        [Authorize]
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUser(string username)
        {
            var user = await _context.UserAccounts.FindAsync(username);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            Console.WriteLine("GET CURRENT USER..."); // Debug

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing in the cookies.");
            }
            Console.WriteLine($"AuthToken: {token}");

            // Display claims for debugging
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            foreach (var claim in allClaims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            // NameIdentifier claim verification
            var subClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (subClaim == null || string.IsNullOrEmpty(subClaim.Value))
            {
                return Unauthorized("Invalid token or missing user information.");
            }

            var username = subClaim.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Invalid token or missing user information. Username is empty.");
            }

            // Retrieve user from DB 
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Create a DTO (Data Transfer Object) with the necessary data (without sensitive data)
            var userDto = new UserDto
            {   
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                BirthDate = user.BirthDate
            };

            // Returns DTO data only
            return Ok(userDto);
        }

        [Authorize]
        [HttpGet("me/statistics")]
        public async Task<IActionResult> GetCurrentUserStatistics()
        {
            Console.WriteLine("GET CURRENT USER STATISTICS..."); // Debug

            var subClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (subClaim == null || string.IsNullOrEmpty(subClaim.Value))
            {
                return Unauthorized("Invalid token or missing user information.");
            }

            var username = subClaim.Value;
            var statistics = await _context.UserStatistics.FirstOrDefaultAsync(s => s.Username == username);
            
            if (statistics == null)
            {
                return NotFound("User statistics not found.");
            }

            return Ok(statistics);
        }

        [Authorize]
        [HttpPut("me/statistics/update")]
        public async Task<IActionResult> UpdateUserStatistics([FromBody] UpdateStatisticsRequest request)
        {
            Console.WriteLine("UPDATING USER STATISTICS..."); // Debug

            var subClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (subClaim == null || string.IsNullOrEmpty(subClaim.Value))
            {
                return Unauthorized("Invalid token or missing user information.");
            }

            var username = subClaim.Value;
            var statistics = await _context.UserStatistics.FirstOrDefaultAsync(s => s.Username == username);

            if (statistics == null)
            {
                return NotFound("User statistics not found.");
            }

            statistics.GamesPlayed += request.GamesPlayed; // Adds the number of games played
            statistics.HighestScore = Math.Max(statistics.HighestScore, request.HighestScore); // Replace if higher
            statistics.Ranking = request.Ranking; // Replaces ranking

            await _context.SaveChangesAsync();
            
            return Ok(statistics);
        }

    }
}

public class UpdateStatisticsRequest
{
    public int GamesPlayed { get; set; }
    public int HighestScore { get; set; }
    public int Ranking { get; set; }
}

public class UserDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime BirthDate { get; set; }
}
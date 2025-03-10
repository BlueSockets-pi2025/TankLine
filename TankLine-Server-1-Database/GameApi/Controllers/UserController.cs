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

            var token = Request.Cookies["AuthToken"]; // Debug 
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing in the cookies.");
            }
            Console.WriteLine(token);
            
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            foreach (var claim in allClaims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            var subClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (subClaim == null)
            {
                return Unauthorized($"Invalid token or missing user information. Claim '{JwtRegisteredClaimNames.Sub}' is missing.");
            }

            var username = subClaim.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized($"Invalid token or missing user information. Claim '{JwtRegisteredClaimNames.Sub}' is empty.");
            }

            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }
    }
}

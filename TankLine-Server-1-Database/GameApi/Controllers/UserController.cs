using GameApi.Data;
using GameApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Action pour tester la connexion au serveur
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Effectuer une requête simple pour vérifier la connexion
                await _context.Database.CanConnectAsync();
                return Ok("Connexion au serveur réussie.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur de connexion : {ex.Message}");
            }
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUser(string username)
        {
            var user = await _context.UserAccounts.FindAsync(username);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
    }
}

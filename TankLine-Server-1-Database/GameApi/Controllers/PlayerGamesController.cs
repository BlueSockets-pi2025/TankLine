using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameApi.Data;
using GameApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace GameApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayedGamesController : ControllerBase
    {
        private readonly GameDbContext _context;

        public PlayedGamesController(GameDbContext context)
        {
            _context = context;
        }

        // GET: api/PlayedGames
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayedGame>>> GetAllGames()
        {
            return await _context.PlayedGames.ToListAsync();
        }

        // GET: api/PlayedGames/by-user/{username}
        [HttpGet("by-user/{username}")]
        public async Task<ActionResult<IEnumerable<PlayedGame>>> GetGamesByUser(string username)
        {
            var games = await _context.PlayedGames
                .Where(pg => pg.Username == username)
                .ToListAsync();

            if (!games.Any())
            {
                return NotFound($"No games found for user '{username}'.");
            }

            return games;
        }

        // POST: api/PlayedGames
        [HttpPost]
        public async Task<ActionResult<PlayedGame>> AddGame(PlayedGame newGame)
        {
            if (!_context.UserAccounts.Any(u => u.Username == newGame.Username))
            {
                return BadRequest("User does not exist.");
            }

            Console.Write("ICIIIIII 1") ; 


            var mapExists = await _context.GeneratedMaps
                                .AnyAsync(m => m.MapId == newGame.MapId);  
            if (!mapExists)
            {
                return BadRequest("Map does not exist.");
            }

            Console.Write("ICIIIIII 2") ; 

            _context.PlayedGames.Add(newGame);
            await _context.SaveChangesAsync();

            Console.Write("ICIIIIII 3") ; 

            return CreatedAtAction(nameof(GetGamesByUser), new { username = newGame.Username }, newGame);
        }

    }

}

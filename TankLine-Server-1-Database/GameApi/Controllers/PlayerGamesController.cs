using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameApi.Data;
using GameApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GameApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protège toutes les routes de ce contrôleur
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

        // GET: api/PlayedGames/by-user
        [HttpGet("by-user/")]
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


        // GET: api/PlayedGames/summary
        [HttpGet("summary/")]
        public async Task<ActionResult<PlayedGameStatsDto>> GetSummary(string username)
        {
            var games = await _context.PlayedGames
                .Where(pg => pg.Username == username)
                .OrderByDescending(pg => pg.GameDate)
                .ToListAsync();

            if (games == null || !games.Any())
            {
                return NotFound();
            }

            var lastGame = games.First();

            var stats = new PlayedGameStatsDto
            {
                Username = lastGame.Username,
                GameWon = lastGame.GameWon,
                TanksDestroyed = lastGame.TanksDestroyed,
                TotalScore = lastGame.TotalScore,
                PlayerRank = lastGame.PlayerRank,
                MapPlayed = lastGame.MapId ?? "Unknown Map",
                GameDate = lastGame.GameDate,
                TotalGames = games.Count,
                TotalVictories = games.Count(g => g.GameWon)
            };

            return Ok(stats);
        }

        // POST: api/PlayedGames
        [HttpPost]
        [AllowAnonymous] // Facultatif : tu peux retirer cette ligne si tu veux aussi sécuriser cette route
        public async Task<ActionResult<PlayedGame>> AddGame(PlayedGame newGame)
        {
            if (!_context.UserAccounts.Any(u => u.Username == newGame.Username))
            {
                return BadRequest("User does not exist.");
            }

            var mapExists = await _context.GeneratedMaps
                                .AnyAsync(m => m.MapId == newGame.MapId);
            if (!mapExists)
            {
                return BadRequest("Map does not exist.");
            }

            _context.PlayedGames.Add(newGame);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGamesByUser), new { username = newGame.Username }, newGame);
        }
    }

    public class PlayedGameStatsDto
    {
        public required string Username { get; set; }
        public bool GameWon { get; set; }
        public int TanksDestroyed { get; set; }
        public int TotalScore { get; set; }
        public int PlayerRank { get; set; }
        public required string MapPlayed { get; set; }
        public DateTime GameDate { get; set; }
        public int TotalGames { get; set; }
        public int TotalVictories { get; set; }
    }
}

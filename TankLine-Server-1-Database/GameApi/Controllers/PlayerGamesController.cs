using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameApi.Data;
using GameApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Castle.Components.DictionaryAdapter;
using System.Diagnostics;

namespace GameApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayedGamesController : ControllerBase
    {
        private readonly GameDbContext _context;

        public PlayedGamesController(GameDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayedGame>>> GetAllGames()
        {
            return await _context.PlayedGames.ToListAsync();
        }

        [HttpGet("summary/")]
        public async Task<ActionResult<PlayedGameStatsDto>> GetSummary()
        {
            var subClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (subClaim == null || string.IsNullOrEmpty(subClaim.Value))
            {
                return Unauthorized("Invalid token or missing user information.");
            }

            var username = subClaim.Value;

            var games = await _context.PlayedGames
                .Where(pg => pg.Username == username)
                .OrderByDescending(pg => pg.GameDate)
                .ToListAsync();

            if (games == null || !games.Any())
            {
                return NotFound("No games found for the current user.");
            }

            var lastGame = games.First();

            var stats = new PlayedGameStatsDto
            {
                Username = username,
                GameWon = lastGame.GameWon,
                TanksDestroyed = games.Sum(g => g.TanksDestroyed),
                TotalScore = lastGame.TotalScore,
                PlayerRank = lastGame.PlayerRank,
                MapPlayed = lastGame.MapId ?? "Unknown Map",
                GameDate = lastGame.GameDate,
                TotalGames = games.Count,
                TotalVictories = games.Count(g => g.GameWon)
            };

            return Ok(stats);
        }


    [HttpPost("addgame/")]
    public async Task<ActionResult<PlayedGame>> AddGame(AddPlayedGameStatsDto newGameDto)
    {
        var subClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (subClaim == null || string.IsNullOrEmpty(subClaim.Value))
        {
            return Unauthorized("Invalid token or missing user information.");
        }
        var username = subClaim.Value;

        if (!_context.UserAccounts.Any(u => u.Username == username))
        {
            return BadRequest("User does not exist.");
        }


        var mapExists = await _context.GeneratedMaps.AnyAsync(m => m.MapId == newGameDto.MapId);
        if (!mapExists)
        {
            return BadRequest("Map does not exist.");
        }


        if (newGameDto.GameDate == default)
        {
            newGameDto.GameDate = DateTime.UtcNow;
        }


        var newGame = new PlayedGame
        {
            Username = username,
            GameDate = newGameDto.GameDate,
            GameWon = newGameDto.GameWon,
            TanksDestroyed = newGameDto.TanksDestroyed,
            TotalScore = newGameDto.TotalScore,
            PlayerRank = newGameDto.PlayerRank,
            MapId = newGameDto.MapId,
        };

        _context.PlayedGames.Add(newGame);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSummary), new { username = username }, newGame);
    }


    }

    public class PlayedGameStatsDto
    {
        public bool GameWon { get; set; }
        public int TanksDestroyed { get; set; }
        public int TotalScore { get; set; }
        public int PlayerRank { get; set; }
        public required string MapPlayed { get; set; }
        public DateTime GameDate { get; set; }
        public int TotalGames { get; set; }
        public int TotalVictories { get; set; }
        public required string Username { get; set; }
    }

    public class AddPlayedGameStatsDto
    {
        public bool GameWon { get; set; }
        public int TanksDestroyed { get; set; }
        public int TotalScore { get; set; }
        public int PlayerRank { get; set; }
        public DateTime GameDate { get; set; }
        public required string MapId { get; set; }

    }




}

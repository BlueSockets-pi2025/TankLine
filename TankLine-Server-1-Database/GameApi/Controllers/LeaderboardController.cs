using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameApi.Data;

namespace GameApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly GameDbContext _context;

        public LeaderboardController(GameDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard()
        {
            // Retrieve users with non-zero high scores, games played, and rankings
            var leaderboard = await _context.UserStatistics
                .Where(s => s.HighestScore > 0 && s.GamesPlayed > 0 && s.Ranking > 0)
                .OrderBy(s => s.Ranking) // Sort by ranking (ascending)
                .Select(s => new
                {
                    Username = s.Username,
                    HighestScore = s.HighestScore,
                    Ranking = s.Ranking,
                    GamesPlayed = s.GamesPlayed
                })
                .ToListAsync();

            return Ok(leaderboard);
        }
    }
}
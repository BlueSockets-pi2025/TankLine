using GameApi.Data;
using GameApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameApi.Controllers
{   
    
    [ApiController]
    [Route("api/[controller]")]
    public class GeneratedMapsController : ControllerBase
    {
        private readonly GameDbContext _context;

        public GeneratedMapsController(GameDbContext context)
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

        [HttpGet("{map_Id}")]
        public async Task<IActionResult> GetMap(string map_Id)
        {
            var map = await _context.GeneratedMaps.FindAsync(map_Id);
            if (map == null)
                return NotFound();
            return Ok(map);
        }
     
    }
}

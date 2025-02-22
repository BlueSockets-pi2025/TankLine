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

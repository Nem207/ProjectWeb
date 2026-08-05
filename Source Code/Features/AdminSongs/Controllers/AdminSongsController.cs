using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.AdminSongs.Services;
namespace SpotifyClone.Features.AdminSongs.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSongsController : Controller
    {
        private readonly SongsService _songsService;
        public AdminSongsController(SongsService songsService)
        {
            _songsService = songsService;
        }
        public async Task<IActionResult> Index()
        {
            var songs = await _songsService.GetAllSongsAsync();
            return View(songs);
        }
        [HttpPost]
        [Route("AdminSongs/Block/{id}")]
        public async Task<IActionResult> Block(int id)
        {
            var (success, message) = await _songsService.BlockSongAsync(id);
            if (!success)
                return BadRequest(new { message });
            return Ok(new { message });
        }
        [HttpPost]
        [Route("AdminSongs/Unblock/{id}")]
        public async Task<IActionResult> Unblock(int id)
        {
            var (success, message) = await _songsService.UnblockSongAsync(id);
            if (!success)
                return BadRequest(new { message });
            return Ok(new { message });
        }
    }
}
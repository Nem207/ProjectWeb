using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.AdminArtists.Services;
namespace SpotifyClone.Features.AdminArtists.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminArtistsController : Controller
    {
        private readonly ArtistsService _artistsService;
        public AdminArtistsController(ArtistsService artistsService)
        {
            _artistsService = artistsService;
        }
        public async Task<IActionResult> Index()
        {
            var artists = await _artistsService.GetAllArtistsAsync();
            return View(artists);
        }
        [HttpPost]
        [Route("AdminArtists/Block/{id}")]
        public async Task<IActionResult> Block(int id)
        {
            var (success, message) = await _artistsService.BlockArtistAsync(id);
            if (!success)
                return BadRequest(new { message });
            return Ok(new { message });
        }
        [HttpPost]
        [Route("AdminArtists/Unblock/{id}")]
        public async Task<IActionResult> Unblock(int id)
        {
            var (success, message) = await _artistsService.UnblockArtistAsync(id);
            if (!success)
                return BadRequest(new { message });
            return Ok(new { message });
        }
    }
}
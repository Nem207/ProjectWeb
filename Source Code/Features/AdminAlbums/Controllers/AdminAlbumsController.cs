using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.AdminAlbums.Services;
namespace SpotifyClone.Features.AdminAlbums.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminAlbumsController : Controller
    {
        private readonly AlbumsService _albumsService;
        public AdminAlbumsController(AlbumsService albumsService)
        {
            _albumsService = albumsService;
        }
        public async Task<IActionResult> Index()
        {
            var albums = await _albumsService.GetAllAlbumsAsync();
            return View(albums);
        }
        [HttpDelete]
        [Route("AdminAlbums/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _albumsService.DeleteAlbumAsync(id);
            if (!success)
                return BadRequest(new { message });
            return Ok(new { message });
        }
    }
}
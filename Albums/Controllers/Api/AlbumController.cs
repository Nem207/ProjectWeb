using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.Album.Services;
namespace SpotifyClone.Features.Album.Controllers
{
    public class AlbumController : Controller
    {
        private readonly IAlbumService _albumService;
        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }
        public async Task<IActionResult> Detail(int id)
        {
            var vm = await _albumService.GetAlbumDetailAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }
    }
}
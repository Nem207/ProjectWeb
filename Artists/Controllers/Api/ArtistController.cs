using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.Artist.Service;
using SpotifyClone.Features.Artist.Services;
namespace SpotifyClone.Features.Artist.Controllers
{
    public class ArtistController : Controller
    {
        private readonly IArtistService _artistService;
        public ArtistController(IArtistService artistService)
        {
            _artistService = artistService;
        }
        public async Task<IActionResult> Detail(int id)
        {
            var vm = await _artistService.GetArtistDetailAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }
    }
}
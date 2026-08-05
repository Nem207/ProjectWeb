using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.Playlist.ViewModels;
namespace SpotifyClone.Features.Playlist.Controllers;
public class PlaylistController : Controller
{
    private readonly SpotifyDbContext _context;
    public PlaylistController(SpotifyDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Detail(int id)
    {
        var exists = await _context.Playlists.AnyAsync(p => p.PlaylistID == id);
        if (!exists)
            return NotFound();
        var vm = new PlaylistDetailVM
        {
            Playlist = new PlaylistVM { PlaylistID = id }
        };
        return View(vm);
    }
}

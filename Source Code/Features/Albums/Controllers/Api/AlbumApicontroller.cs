using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.Album.Services;
namespace SpotifyClone.Features.Album.Controllers.Api
{
    [Route("api/Album")]
    [ApiController]
    public class AlbumApicontroller : ControllerBase
    {
        private readonly IAlbumService _albumService;
        public AlbumApicontroller(IAlbumService albumService)
        {
            _albumService = albumService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var albums = await _albumService.GetAllAsync();
            return Ok(albums);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var album = await _albumService.GetByIdAsync(id);
            if (album == null)
                return NotFound(new { message = "Không tìm thấy album" });
            return Ok(album);
        }
        [HttpGet("{id}/artists")]
        public async Task<IActionResult> GetArtists(int id)
        {
            var artists = await _albumService.GetArtistsAsync(id);
            return Ok(artists);
        }
        [HttpGet("{id}/songs")]
        public async Task<IActionResult> GetSongs(int id)
        {
            var songs = await _albumService.GetSongsAsync(id);
            return Ok(songs);
        }
    }
}
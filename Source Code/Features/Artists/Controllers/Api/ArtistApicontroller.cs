using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.Artist.Service;
using SpotifyClone.Features.Artist.Services;
using SpotifyClone.Features.Auth.Services;
namespace SpotifyClone.Features.Artist.Controllers.Api
{
    [Route("api/Artist")]
    [ApiController]
    public class ArtistApicontroller : ControllerBase
    {
        private readonly IArtistService _artistService;
        private readonly ICurrentUserService _currentUser;
        public ArtistApicontroller(IArtistService artistService, ICurrentUserService currentUser)
        {
            _artistService = artistService;
            _currentUser = currentUser;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var artists = await _artistService.GetAllAsync();
            return Ok(artists);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var artist = await _artistService.GetByIdAsync(id);
            if (artist == null)
                return NotFound(new { message = "Không tìm thấy nghệ sĩ" });
            return Ok(artist);
        }
        [HttpGet("{id}/albums")]
        public async Task<IActionResult> GetAlbums(int id)
        {
            var albums = await _artistService.GetAlbumsAsync(id);
            return Ok(albums);
        }
        [HttpGet("{id}/songs")]
        public async Task<IActionResult> GetSongs(int id)
        {
            var songs = await _artistService.GetSongsAsync(id);
            return Ok(songs);
        }
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetStats(int id)
        {
            var stats = await _artistService.GetStatsAsync(id);
            return Ok(stats);
        }
        [HttpGet("{id}/follow")]
        public async Task<IActionResult> IsFollowing(int id)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId is not int userId)
                return Ok(new { following = false }); 
            var following = await _artistService.IsFollowingAsync(userId, id);
            return Ok(new { following });
        }
        [HttpPost("{id}/follow/toggle")]
        public async Task<IActionResult> ToggleFollow(int id)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId is not int userId)
                return Unauthorized(new { message = "Vui lòng đăng nhập để theo dõi nghệ sĩ." });
            var following = await _artistService.ToggleFollowAsync(userId, id);
            return Ok(new { following });
        }
        [HttpGet("followed")]
        public async Task<IActionResult> GetFollowed()
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId is not int userId)
                return Ok(new List<object>());
            var artists = await _artistService.GetFollowedArtistsAsync(userId);
            return Ok(artists);
        }
        [HttpGet("{id}/block")]
        public async Task<IActionResult> IsBlocked(int id)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId is not int userId)
                return Ok(new { blocked = false }); 
            var blocked = await _artistService.IsBlockedAsync(userId, id);
            return Ok(new { blocked });
        }
        [HttpPost("{id}/block/toggle")]
        public async Task<IActionResult> ToggleBlock(int id)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId is not int userId)
                return Unauthorized(new { message = "Vui lòng đăng nhập để chặn nghệ sĩ." });
            var blocked = await _artistService.ToggleBlockAsync(userId, id);
            return Ok(new { blocked });
        }
        [HttpGet("blocked")]
        public async Task<IActionResult> GetBlocked()
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId is not int userId)
                return Ok(new List<int>());
            var ids = await _artistService.GetBlockedArtistIdsAsync(userId);
            return Ok(ids);
        }
    }
}
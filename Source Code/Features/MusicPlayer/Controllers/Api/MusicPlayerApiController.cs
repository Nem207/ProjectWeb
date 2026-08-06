using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.MusicPlayer.Services;
using SpotifyClone.Features.MusicPlayer.ViewModels;
using SpotifyClone.Features.Auth.Services;
namespace SpotifyClone.Features.MusicPlayer.Controllers.Api
{
    [Route("api/musicplayer")]
    [ApiController]
    public class MusicPlayerApiController : ControllerBase
    {
        private readonly IMusicPlayerService _musicPlayerService;
        private readonly ICurrentUserService _currentUser;
        public MusicPlayerApiController(IMusicPlayerService musicPlayerService, ICurrentUserService currentUser)
        {
            _musicPlayerService = musicPlayerService;
            _currentUser = currentUser;
        }
        [HttpGet("song/{id}")]
        public async Task<IActionResult> GetSong(int id)
        {
            var song = await _musicPlayerService.GetSongAsync(id, _currentUser.UserId);
            if (song == null)
            {
                return NotFound(new { message = "Song not found." });
            }
            return Ok(song);
        }
        [HttpPost("play/{id}")]
        public async Task<IActionResult> Play(int id)
        {
            var success = await _musicPlayerService.IncrementPlayCountAsync(id, _currentUser.UserId);
            if (!success)
            {
                return NotFound(new { message = "Song not found." });
            }
            return Ok(new { message = "Play count updated." });
        }
        [HttpPost("earn/{id}")]
        public async Task<IActionResult> Earn(int id)
        {
            var success = await _musicPlayerService.RegisterListenEarningAsync(id, _currentUser.UserId);
            if (!success)
            {
                // Không tính tiền (bài không hợp lệ, chưa duyệt, hoặc đang trong thời gian cooldown chống gian lận).
                return Ok(new { earned = false });
            }
            return Ok(new { earned = true });
        }
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending([FromQuery] int take = 10)
        {
            var songs = await _musicPlayerService.GetTrendingAsync(take);
            return Ok(songs);
        }
        [HttpDelete("history")]
        public async Task<IActionResult> ClearHistory()
        {
            if (_currentUser.UserId is not int userId)
            {
                return Unauthorized();
            }
            await _musicPlayerService.ClearHistoryAsync(userId);
            return Ok(new { message = "History cleared." });
        }
    }
}
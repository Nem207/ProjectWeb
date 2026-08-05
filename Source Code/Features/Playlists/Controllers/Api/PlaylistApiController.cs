using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Models;
using SpotifyClone.Features.Playlist.ViewModels;
using SpotifyClone.Features.Song.ViewModels;
using SpotifyClone.Features.Auth.Services;
using SpotifyClone.Features.Premium.Services;
using PlaylistModel = SpotifyClone.Models.Playlist;
namespace SpotifyClone.Features.Playlist.Controllers.Api;

[ApiController]
[Route("api/playlists")]
public class PlaylistApiController : ControllerBase
{
    private readonly SpotifyDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPremiumService _premiumService;
    public const string FavoritesPlaylistName = "Bài hát yêu thích";
    public const int FreeMaxSongsPerPlaylist = 5;
    public PlaylistApiController(SpotifyDbContext context, ICurrentUserService currentUser, IPremiumService premiumService)
    {
        _context = context;
        _currentUser = currentUser;
        _premiumService = premiumService;
    }
    [HttpGet]
    public async Task<IActionResult> GetMyPlaylists()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
        {
            return Ok(new List<PlaylistVM>());
        }
        var playlists = await _context.Playlists
            .Where(p => p.UserID == userId)
            .OrderByDescending(p => p.PlaylistName == FavoritesPlaylistName)
            .ThenByDescending(p => p.CreatedAt)
            .Select(p => new PlaylistVM
            {
                PlaylistID = p.PlaylistID,
                PlaylistName = p.PlaylistName,
                CoverImage = p.CoverImage,
                Description = p.Description,
                IsPublic = p.IsPublic,
                CreatedAt = p.CreatedAt,
                SongCount = p.PlaylistSongs.Count(ps => ps.Song.Status == SongStatus.Approved)
            })
            .ToListAsync();
        return Ok(playlists);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlaylist(int id)
    {
        var playlist = await _context.Playlists
            .Include(p => p.User)
            .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                    .ThenInclude(s => s.SongArtists)
                        .ThenInclude(sa => sa.Artist)
            .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                    .ThenInclude(s => s.Album)
            .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                    .ThenInclude(s => s.SongQualities)
            .FirstOrDefaultAsync(p => p.PlaylistID == id);
        if (playlist == null)
            return NotFound();
        var orderedSongs = playlist.PlaylistSongs
            .Where(ps => ps.Song.Status == SongStatus.Approved)
            .OrderBy(x => x.AddedAt).ToList();
        var vm = new PlaylistDetailVM
        {
            Playlist = new PlaylistVM
            {
                PlaylistID = playlist.PlaylistID,
                PlaylistName = playlist.PlaylistName,
                CoverImage = playlist.CoverImage,
                Description = playlist.Description,
                IsPublic = playlist.IsPublic,
                CreatedAt = playlist.CreatedAt,
                OwnerName = playlist.User?.FullName ?? playlist.User?.Username ?? "Người dùng",
                SongCount = orderedSongs.Count,
                TotalDurationSeconds = orderedSongs.Sum(x => x.Song.Duration)
            },
            Songs = orderedSongs
                .Select(x => new SongVM
                {
                    SongID = x.Song.SongID,
                    Title = x.Song.Title,
                    Artist = x.Song.SongArtists
                        .Select(a => a.Artist.ArtistName)
                        .FirstOrDefault() ?? "",
                    ArtistID = x.Song.SongArtists
                        .Select(a => (int?)a.Artist.ArtistID)
                        .FirstOrDefault(),
                    Album = x.Song.Album?.AlbumName ?? "",
                    CoverImage = x.Song.CoverImage,
                    Duration = x.Song.Duration,
                    AudioUrl = x.Song.AudioURL ?? ""
                })
                .ToList()
        };
        return Ok(vm);
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(PlaylistVM vm)
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();
        PlaylistModel playlist = new PlaylistModel
        {
            UserID = userId,
            PlaylistName = vm.PlaylistName,
            CoverImage = vm.CoverImage,
            Description = vm.Description,
            IsPublic = vm.IsPublic,
            CreatedAt = DateTime.Now
        };
        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();
        return Ok(playlist.PlaylistID);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PlaylistVM vm)
    {
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(x => x.PlaylistID == id);
        if (playlist == null)
            return NotFound();
        playlist.PlaylistName = vm.PlaylistName;
        playlist.CoverImage = vm.CoverImage;
        playlist.Description = vm.Description;
        playlist.IsPublic = vm.IsPublic;
        await _context.SaveChangesAsync();
        return Ok();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(x => x.PlaylistID == id);
        if (playlist == null)
            return NotFound();
        _context.Playlists.Remove(playlist);
        await _context.SaveChangesAsync();
        return Ok();
    }
    [HttpPost("{playlistId}/songs/{songId}")]
    [Authorize]
    public async Task<IActionResult> AddSong(int playlistId, int songId)
    {
        bool exists = await _context.PlaylistSongs
            .AnyAsync(x => x.PlaylistID == playlistId &&
                           x.SongID == songId);
        if (exists)
            return Ok(new { added = false });
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.PlaylistID == playlistId);
        if (playlist == null)
            return NotFound(new { message = "Playlist không tồn tại." });

        if (playlist.PlaylistName != FavoritesPlaylistName)
        {
            var userId = _currentUser.UserId;
            bool isPremium = userId.HasValue && await _premiumService.HasPremiumAsync(userId.Value);
            if (!isPremium)
            {
                int currentCount = await _context.PlaylistSongs
                    .CountAsync(x => x.PlaylistID == playlistId);
                if (currentCount >= FreeMaxSongsPerPlaylist)
                {
                    return StatusCode(403, new
                    {
                        message = $"Tài khoản miễn phí chỉ thêm được tối đa {FreeMaxSongsPerPlaylist} bài/playlist. Nâng cấp Premium để thêm không giới hạn.",
                        limitReached = true
                    });
                }
            }
        }

        _context.PlaylistSongs.Add(new PlaylistSong
        {
            PlaylistID = playlistId,
            SongID = songId,
            AddedAt = DateTime.Now
        });
        await _context.SaveChangesAsync();
        return Ok(new { added = true });
    }
    [HttpGet("containing/{songId}")]
    public async Task<IActionResult> GetPlaylistsContainingSong(int songId)
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Ok(new List<int>());
        var playlistIds = await _context.PlaylistSongs
            .Where(ps => ps.SongID == songId && ps.Playlist.UserID == userId)
            .Select(ps => ps.PlaylistID)
            .ToListAsync();
        return Ok(playlistIds);
    }
    [HttpGet("favorites")]
    [Authorize]
    public async Task<IActionResult> PeekFavoritesPlaylist()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.UserID == userId && p.PlaylistName == FavoritesPlaylistName);
        return Ok(playlist?.PlaylistID);
    }
    [HttpPost("favorites")]
    [Authorize]
    public async Task<IActionResult> GetOrCreateFavoritesPlaylist()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.UserID == userId && p.PlaylistName == FavoritesPlaylistName);
        if (playlist == null)
        {
            playlist = new PlaylistModel
            {
                UserID = userId,
                PlaylistName = FavoritesPlaylistName,
                IsPublic = false,
                CreatedAt = DateTime.Now
            };
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();
        }
        return Ok(playlist.PlaylistID);
    }
    [HttpDelete("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> RemoveSong(int playlistId, int songId)
    {
        var item = await _context.PlaylistSongs
            .FirstOrDefaultAsync(x => x.PlaylistID == playlistId &&
                                      x.SongID == songId);
        if (item == null)
            return NotFound();
        _context.PlaylistSongs.Remove(item);
        await _context.SaveChangesAsync();
        return Ok();
    }
    [HttpGet("{playlistId}/available-songs")]
    public async Task<IActionResult> GetAvailableSongs(int playlistId, string? search, int? genreId)
    {
        var existingSongIds = await _context.PlaylistSongs
            .Where(x => x.PlaylistID == playlistId)
            .Select(x => x.SongID)
            .ToListAsync();
        var query = _context.Songs
            .Include(s => s.Album)
            .Include(s => s.SongArtists)
                .ThenInclude(sa => sa.Artist)
            .Where(s => s.Status == SongStatus.Approved);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string keyword = search.Trim().ToLower();
            query = query.Where(s =>
                s.Title.ToLower().Contains(keyword) ||
                s.SongArtists.Any(sa => sa.Artist.ArtistName.ToLower().Contains(keyword)));
        }
        if (genreId.HasValue)
        {
            query = query.Where(s => s.SongGenres.Any(sg => sg.GenreID == genreId.Value));
        }
        var songs = await query
            .OrderBy(s => s.Title)
            .Take(50)
            .Select(s => new SongVM
            {
                SongID = s.SongID,
                Title = s.Title,
                Artist = s.SongArtists.Select(a => a.Artist.ArtistName).FirstOrDefault() ?? "",
                ArtistID = s.SongArtists.Any() ? s.SongArtists.Select(a => a.Artist.ArtistID).FirstOrDefault() : (int?)null,
                Album = s.Album != null ? s.Album.AlbumName : "",
                CoverImage = s.CoverImage,
                Duration = s.Duration,
                IsAdded = existingSongIds.Contains(s.SongID)
            })
            .ToListAsync();
        return Ok(songs);
    }
    [HttpPut("{playlistId}/songs")]
    public async Task<IActionResult> ReorderSongs(
        int playlistId,
        PlaylistSongOrderVM vm)
    {
        var songs = await _context.PlaylistSongs
            .Where(x => x.PlaylistID == playlistId)
            .ToListAsync();
        foreach (var item in songs)
        {
            item.AddedAt = DateTime.Now.AddSeconds(
                vm.SongIDs.IndexOf(item.SongID));
        }
        await _context.SaveChangesAsync();
        return Ok();
    }
}
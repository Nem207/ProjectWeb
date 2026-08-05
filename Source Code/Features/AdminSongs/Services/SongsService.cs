using Microsoft.EntityFrameworkCore;
using SpotifyClone.Features.AdminSongs.ViewModels;
using SpotifyClone.Data;
using SpotifyClone.Models;
namespace SpotifyClone.Features.AdminSongs.Services
{
    public class SongsService
    {
        private readonly SpotifyDbContext _context;
        public SongsService(SpotifyDbContext context)
        {
            _context = context;
        }
        public async Task<List<SongsViewModel>> GetAllSongsAsync()
        {
            var songs = await _context.Songs
                .Include(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                .ToListAsync();
            return songs.Select(s => new SongsViewModel
            {
                SongID = s.SongID,
                Title = s.Title,
                Duration = s.Duration,
                CreatedAt = s.CreatedAt,
                IsPremium = s.IsPremium,
                Status = s.Status,
                ArtistName = s.SongArtists.Any()
                    ? string.Join(", ", s.SongArtists.Select(sa => sa.Artist.ArtistName ?? "Chưa có tên"))
                    : "Chưa có nghệ sĩ"
            }).ToList();
        }
        public async Task<(bool Success, string Message)> BlockSongAsync(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return (false, "Không tìm thấy bài hát.");
            if (song.Status == SongStatus.Blocked)
                return (false, "Bài hát này đã bị chặn trước đó.");
            song.Status = SongStatus.Blocked;
            await _context.SaveChangesAsync();
            return (true, "Đã chặn bài hát. Bài hát sẽ không hiển thị với người dùng.");
        }
        public async Task<(bool Success, string Message)> UnblockSongAsync(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return (false, "Không tìm thấy bài hát.");
            if (song.Status != SongStatus.Blocked)
                return (false, "Bài hát này không ở trạng thái bị chặn.");
            song.Status = SongStatus.Approved;
            await _context.SaveChangesAsync();
            return (true, "Đã bỏ chặn bài hát.");
        }
    }
}
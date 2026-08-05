using Microsoft.EntityFrameworkCore;
using SpotifyClone.Features.AdminSongApproval.ViewModels;
using SpotifyClone.Data;
namespace SpotifyClone.Features.AdminSongApproval.Services
{
    public class SongApprovalService
    {
        private readonly SpotifyDbContext _context;
        public SongApprovalService(SpotifyDbContext context)
        {
            _context = context;
        }
        public async Task<List<SongApprovalViewModel>> GetPendingSongsAsync()
        {
            var songs = await _context.Songs
                .Where(s => s.Status == "Pending")
                .Include(s => s.SongArtists).ThenInclude(sa => sa.Artist)
                .Include(s => s.Album)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();
            return songs.Select(s => new SongApprovalViewModel
            {
                SongID = s.SongID,
                Title = s.Title,
                CoverImage = s.CoverImage,
                AudioURL = s.AudioURL,
                Duration = s.Duration,
                AlbumTitle = s.Album?.AlbumName,
                CreatedAt = s.CreatedAt,
                Status = s.Status,
                IsPremium = s.IsPremium,
                ArtistName = s.SongArtists.Any()
                    ? string.Join(", ", s.SongArtists.Select(sa => sa.Artist.ArtistName))
                    : "Chưa rõ"
            }).ToList();
        }
        public async Task<(bool Success, string Message)> ApproveSongAsync(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return (false, "Không tìm thấy bài hát.");
            if (song.Status != "Pending")
                return (false, "Bài hát này đã được xử lý trước đó.");
            song.Status = "Approved";
            song.RejectReason = null;
            await _context.SaveChangesAsync();
            return (true, "Duyệt bài hát thành công.");
        }
        public async Task<(bool Success, string Message)> RejectSongAsync(int id, string reason)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return (false, "Không tìm thấy bài hát.");
            if (song.Status != "Pending")
                return (false, "Bài hát này đã được xử lý trước đó.");
            if (string.IsNullOrWhiteSpace(reason))
                return (false, "Vui lòng nhập lý do từ chối.");
            song.Status = "Rejected";
            song.RejectReason = reason;
            await _context.SaveChangesAsync();
            return (true, "Đã từ chối bài hát.");
        }
    }
}
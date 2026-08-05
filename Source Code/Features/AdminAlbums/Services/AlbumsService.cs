using Microsoft.EntityFrameworkCore;
using SpotifyClone.Features.AdminAlbums.ViewModels;
using SpotifyClone.Data;
namespace SpotifyClone.Features.AdminAlbums.Services
{
    public class AlbumsService
    {
        private readonly SpotifyDbContext _context;
        public AlbumsService(SpotifyDbContext context)
        {
            _context = context;
        }
        public async Task<List<AlbumsViewModel>> GetAllAlbumsAsync()
        {
            var albums = await _context.Albums
                .Include(a => a.AlbumArtists)
                    .ThenInclude(aa => aa.Artist)
                .OrderByDescending(a => a.ReleaseDate)
                .ToListAsync();
            return albums.Select(a => new AlbumsViewModel
            {
                AlbumID = a.AlbumID,
                AlbumTitle = a.AlbumName,
                ReleaseDate = a.ReleaseDate,
                ArtistIDs = a.AlbumArtists.Select(aa => aa.ArtistID).ToList(),
                ArtistName = a.AlbumArtists.Any()
                    ? string.Join(", ", a.AlbumArtists.Select(aa => aa.Artist.ArtistName))
                    : "Chưa có nghệ sĩ"
            }).ToList();
        }
        public async Task<(bool Success, string Message)> DeleteAlbumAsync(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album == null)
                return (false, "Không tìm thấy album.");
            try
            {
                var albumArtists = _context.AlbumArtists.Where(aa => aa.AlbumID == id);
                _context.AlbumArtists.RemoveRange(albumArtists);
                var songsInAlbum = await _context.Songs
                    .Where(s => s.AlbumID == id)
                    .ToListAsync();
                foreach (var song in songsInAlbum)
                {
                    song.AlbumID = null;
                }
                await _context.SaveChangesAsync();
                _context.Albums.Remove(album);
                await _context.SaveChangesAsync();
                return (true, "Xóa album thành công.");
            }
            catch (DbUpdateException)
            {
                return (false, "Đã xảy ra lỗi khi xóa album. Vui lòng thử lại hoặc liên hệ quản trị viên.");
            }
        }
    }
}
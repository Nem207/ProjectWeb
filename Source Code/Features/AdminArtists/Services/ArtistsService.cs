using Microsoft.EntityFrameworkCore;
using SpotifyClone.Features.AdminArtists.ViewModels;
using SpotifyClone.Data;
namespace SpotifyClone.Features.AdminArtists.Services
{
    public class ArtistsService
    {
        private readonly SpotifyDbContext _context;
        public ArtistsService(SpotifyDbContext context)
        {
            _context = context;
        }
        public async Task<List<ArtistsViewModel>> GetAllArtistsAsync()
        {
            return await _context.Artists
                .OrderByDescending(a => a.ArtistID)
                .Select(a => new ArtistsViewModel
                {
                    ArtistID = a.ArtistID,
                    ArtistName = a.ArtistName ?? "Chưa có tên",
                    IsBlocked = a.IsBlocked
                })
                .ToListAsync();
        }
        public async Task<(bool Success, string Message)> BlockArtistAsync(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null)
                return (false, "Không tìm thấy nghệ sĩ.");
            if (artist.IsBlocked)
                return (false, "Nghệ sĩ này đã bị chặn trước đó.");
            artist.IsBlocked = true;
            await _context.SaveChangesAsync();
            return (true, "Đã chặn nghệ sĩ. Nghệ sĩ và bài hát của họ sẽ không hiển thị với người dùng.");
        }
        public async Task<(bool Success, string Message)> UnblockArtistAsync(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null)
                return (false, "Không tìm thấy nghệ sĩ.");
            if (!artist.IsBlocked)
                return (false, "Nghệ sĩ này không ở trạng thái bị chặn.");
            artist.IsBlocked = false;
            await _context.SaveChangesAsync();
            return (true, "Đã bỏ chặn nghệ sĩ.");
        }
    }
}
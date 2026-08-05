using Microsoft.EntityFrameworkCore;
using SpotifyClone.Features.AdminUsers.ViewModels;
using SpotifyClone.Data;
namespace SpotifyClone.Features.AdminUsers.Services
{
    public class UsersService
    {
        private readonly SpotifyDbContext _context;
        public UsersService(SpotifyDbContext context)
        {
            _context = context;
        }
        public async Task<List<UsersViewModel>> GetAllUsersAsync()
        {
            var now = DateTime.Now;
            return await _context.Users
                .Select(u => new UsersViewModel
                {
                    UserID = u.UserID,
                    UserName = u.Username,
                    AvatarURL = u.AvatarUrl,
                    IsPremium = u.UserSubscriptions.Any(s =>
                        s.Status == "Active" && s.EndDate >= now),
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }
        public async Task<(bool Success, string Message)> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return (false, "Không tìm thấy người dùng.");
            try
            {
                var ownedArtists = await _context.Artists
                    .Where(a => a.UserID == id)
                    .ToListAsync();
                foreach (var artist in ownedArtists)
                {
                    artist.UserID = null;
                }
                _context.Downloads.RemoveRange(
                    _context.Downloads.Where(d => d.UserID == id));
                _context.ListeningHistories.RemoveRange(
                    _context.ListeningHistories.Where(lh => lh.UserID == id));
                _context.Notifications.RemoveRange(
                    _context.Notifications.Where(n => n.UserID == id));
                _context.Payments.RemoveRange(
                    _context.Payments.Where(p => p.UserID == id));
                var ownedPlaylistIds = await _context.Playlists
                    .Where(p => p.UserID == id)
                    .Select(p => p.PlaylistID)
                    .ToListAsync();
                if (ownedPlaylistIds.Any())
                {
                    _context.PlaylistSongs.RemoveRange(
                        _context.PlaylistSongs.Where(ps => ownedPlaylistIds.Contains(ps.PlaylistID)));
                    _context.PlaylistMembers.RemoveRange(
                        _context.PlaylistMembers.Where(pm => ownedPlaylistIds.Contains(pm.PlaylistID)));
                }
                _context.PlaylistMembers.RemoveRange(
                    _context.PlaylistMembers.Where(pm => pm.UserID == id));
                if (ownedPlaylistIds.Any())
                {
                    var ownedPlaylists = await _context.Playlists
                        .Where(p => ownedPlaylistIds.Contains(p.PlaylistID))
                        .ToListAsync();
                    _context.Playlists.RemoveRange(ownedPlaylists);
                }
                _context.SearchHistories.RemoveRange(
                    _context.SearchHistories.Where(sh => sh.UserID == id));
                _context.UserFollowArtists.RemoveRange(
                    _context.UserFollowArtists.Where(ufa => ufa.UserID == id));
                _context.UserQueues.RemoveRange(
                    _context.UserQueues.Where(q => q.UserID == id));
                _context.UserSubscriptions.RemoveRange(
                    _context.UserSubscriptions.Where(us => us.UserID == id));
                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM UserAdHistory WHERE UserID = {id}");
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return (true, "Xóa người dùng thành công.");
            }
            catch (DbUpdateException)
            {
                return (false, "Đã xảy ra lỗi khi xóa người dùng. Vui lòng thử lại hoặc liên hệ quản trị viên.");
            }
        }
    }
}
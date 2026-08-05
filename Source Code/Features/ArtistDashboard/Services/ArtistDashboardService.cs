using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.ArtistDashboard.ViewModels;
namespace SpotifyClone.Features.ArtistDashboard.Services;
public interface IArtistDashboardService
{
    Task<ArtistDashboardViewModel?> GetDashboardForUserAsync(int userId);
    Task<bool> CreateSongAsync(int userId, UploadSongViewModel input);
    Task<List<ArtistNotificationItem>?> GetNotificationsForUserAsync(int userId);
}
public class ArtistDashboardService : IArtistDashboardService
{
    private readonly SpotifyDbContext _context;
    public ArtistDashboardService(SpotifyDbContext context)
    {
        _context = context;
    }
    public async Task<ArtistDashboardViewModel?> GetDashboardForUserAsync(int userId)
    {
        var artist = await _context.Artists
            .Include(a => a.ArtistStatistic)
            .FirstOrDefaultAsync(a => a.UserID == userId);
        if (artist == null)
        {
            return null;
        }
        var songIds = await _context.SongArtists
            .Where(sa => sa.ArtistID == artist.ArtistID)
            .Select(sa => sa.SongID)
            .ToListAsync();
        var songs = await _context.Songs
            .Where(s => songIds.Contains(s.SongID))
            .ToListAsync();
        var stats = await _context.SongStatistics
            .Where(s => songIds.Contains(s.SongID))
            .ToListAsync();
        var topSongs = songs
            .Select(s =>
            {
                var stat = stats.FirstOrDefault(x => x.SongID == s.SongID);
                return new ArtistTopSong
                {
                    SongId = s.SongID,
                    Title = s.Title,
                    CoverImage = s.CoverImage,
                    Status = s.Status,
                    IsPremium = s.IsPremium,
                    TotalPlays = stat?.TotalPlays ?? 0,
                    TotalLikes = stat?.TotalLikes ?? 0
                };
            })
            .OrderByDescending(s => s.TotalPlays)
            .Take(10)
            .ToList();
        return new ArtistDashboardViewModel
        {
            ArtistID = artist.ArtistID,
            ArtistName = artist.ArtistName ?? "",
            Avatar = artist.Avatar,
            TotalSongs = songs.Count,
            TotalPlays = stats.Sum(s => s.TotalPlays),
            TotalLikes = stats.Sum(s => s.TotalLikes),
            MonthlyListeners = artist.ArtistStatistic?.MonthlyListeners ?? 0,
            TotalFollowers = artist.ArtistStatistic?.TotalFollowers ?? 0,
            TopSongs = topSongs
        };
    }
    public async Task<bool> CreateSongAsync(int userId, UploadSongViewModel input)
    {
        var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
        if (artist == null)
        {
            return false;
        }
        var song = new Models.Song
        {
            Title = input.Title,
            Duration = input.Duration,
            AudioURL = input.AudioURL,
            CoverImage = input.CoverImage,
            IsPremium = input.IsPremium,
            ReleaseDate = DateTime.Now,
            CreatedAt = DateTime.Now,
            Status = Models.SongStatus.Pending 
        };
        _context.Songs.Add(song);
        await _context.SaveChangesAsync(); 
        _context.SongArtists.Add(new Models.SongArtist
        {
            SongID = song.SongID,
            ArtistID = artist.ArtistID
        });
        _context.SongStatistics.Add(new Models.SongStatistic
        {
            SongID = song.SongID,
            TotalPlays = 0,
            TotalLikes = 0,
            TotalDownloads = 0
        });
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<List<ArtistNotificationItem>?> GetNotificationsForUserAsync(int userId)
    {
        var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
        if (artist == null)
        {
            return null;
        }
        var songIds = await _context.SongArtists
            .Where(sa => sa.ArtistID == artist.ArtistID)
            .Select(sa => sa.SongID)
            .ToListAsync();
        var notifications = await _context.Songs
            .Where(s => songIds.Contains(s.SongID) &&
                        (s.Status == Models.SongStatus.Approved || s.Status == Models.SongStatus.Rejected))
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new ArtistNotificationItem
            {
                SongId = s.SongID,
                Title = s.Title,
                CoverImage = s.CoverImage,
                Status = s.Status,
                RejectReason = s.RejectReason,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
        return notifications;
    }
}
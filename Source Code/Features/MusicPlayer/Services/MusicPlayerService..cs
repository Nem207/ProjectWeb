using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Models;
using SpotifyClone.Features.MusicPlayer.ViewModels;
using SpotifyClone.Features.Premium.Services;
namespace SpotifyClone.Features.MusicPlayer.Services;

public class MusicPlayerService : IMusicPlayerService
{
    private readonly SpotifyDbContext _context;
    private readonly IPremiumService _premiumService;
    public const double FreePreviewRatio = 0.3;
    public MusicPlayerService(SpotifyDbContext context, IPremiumService premiumService)
    {
        _context = context;
        _premiumService = premiumService;
    }
    public async Task<NowPlayingVM?> GetSongAsync(int songId, int? userId)
    {
        var song = await _context.Songs
            .Include(s => s.Album)
            .Include(s => s.SongStatistic)
            .Include(s => s.SongArtists)
                .ThenInclude(sa => sa.Artist)
            .Include(s => s.SongQualities)
            .FirstOrDefaultAsync(s => s.SongID == songId);
        if (song == null)
        {
            return null;
        }
        int? maxPreviewSeconds = null;
        if (song.IsPremium)
        {
            bool isPremiumUser = userId.HasValue && await _premiumService.HasPremiumAsync(userId.Value);
            if (!isPremiumUser && song.Duration > 0)
            {
                maxPreviewSeconds = (int)Math.Floor(song.Duration * FreePreviewRatio);
                if (maxPreviewSeconds < 1) maxPreviewSeconds = 1;
            }
        }
        return new NowPlayingVM
        {
            SongID = song.SongID,
            Title = song.Title,
            CoverImage = song.CoverImage,
            Duration = song.Duration,
            IsPremium = song.IsPremium,
            MaxPreviewSeconds = maxPreviewSeconds,
            AlbumName = song.Album?.AlbumName,
            ArtistNames = string.Join(", ", song.SongArtists.Select(sa => sa.Artist!.ArtistName)),
            MainArtistID = song.SongArtists.Select(sa => (int?)sa.ArtistID).FirstOrDefault(),
            AudioUrl = song.AudioURL,
            TotalPlays = song.SongStatistic?.TotalPlays ?? 0,
            Qualities = song.SongQualities.Select(q => new SongQualityVM
            {
                QualityName = q.QualityName,
                Bitrate = q.Bitrate,
                FileUrl = q.FileUrl
            }).ToList()
        };
    }
    public async Task<bool> IncrementPlayCountAsync(int songId, int? userId)
    {
        var song = await _context.Songs
            .Include(s => s.SongStatistic)
            .FirstOrDefaultAsync(s => s.SongID == songId);
        if (song == null)
        {
            return false;
        }
        if (song.SongStatistic == null)
        {
            _context.SongStatistics.Add(new SongStatistic
            {
                SongID = songId,
                TotalPlays = 1,
                TotalLikes = 0,
                TotalDownloads = 0
            });
        }
        else
        {
            song.SongStatistic.TotalPlays += 1;
        }
        if (userId.HasValue)
        {
            var lastEntry = await _context.ListeningHistories
                .Where(h => h.UserID == userId.Value)
                .OrderByDescending(h => h.PlayedAt)
                .FirstOrDefaultAsync();
            if (lastEntry != null && lastEntry.SongID == songId)
            {
                lastEntry.PlayedAt = DateTime.UtcNow;
            }
            else
            {
                _context.ListeningHistories.Add(new ListeningHistory
                {
                    UserID = userId.Value,
                    SongID = songId,
                    PlayedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                await TrimHistoryAsync(userId.Value);
            }
        }
        await _context.SaveChangesAsync();
        return true;
    }
    /// <summary>Số tiền (VNĐ) trả cho nghệ sĩ mỗi lượt nghe hợp lệ.</summary>
    public const decimal EarningPerPlay = 1000m;
    /// <summary>Ngưỡng thời gian nghe tối thiểu (giây) để một lượt nghe được tính tiền, chống gian lận (bấm phát rồi bỏ ngay).</summary>
    public const int MinListenSecondsForEarning = 30;
    /// <summary>Khoảng thời gian tối thiểu giữa 2 lần được tính tiền cho cùng 1 người nghe + 1 bài hát, tránh spam gọi API.</summary>
    private static readonly TimeSpan EarningCooldown = TimeSpan.FromMinutes(2);

    public async Task<bool> RegisterListenEarningAsync(int songId, int? userId)
    {
        var song = await _context.Songs
            .Include(s => s.SongArtists)
            .FirstOrDefaultAsync(s => s.SongID == songId);
        if (song == null || song.Status != SongStatus.Approved)
        {
            return false;
        }
        var artistIds = song.SongArtists.Select(sa => sa.ArtistID).Distinct().ToList();
        if (artistIds.Count == 0)
        {
            return false;
        }
        if (userId.HasValue)
        {
            var cooldownStart = DateTime.UtcNow - EarningCooldown;
            bool recentlyEarned = await _context.ArtistEarnings
                .AnyAsync(e => e.SongID == songId &&
                               e.UserID == userId.Value &&
                               e.CreatedAt > cooldownStart);
            if (recentlyEarned)
            {
                return false;
            }
        }
        var shareAmount = Math.Round(EarningPerPlay / artistIds.Count, 2);
        var now = DateTime.UtcNow;
        foreach (var artistId in artistIds)
        {
            var stat = await _context.ArtistStatistics
                .FirstOrDefaultAsync(x => x.ArtistID == artistId);
            if (stat == null)
            {
                stat = new ArtistStatistic { ArtistID = artistId };
                _context.ArtistStatistics.Add(stat);
            }
            stat.TotalEarnings += shareAmount;
            _context.ArtistEarnings.Add(new ArtistEarning
            {
                ArtistID = artistId,
                SongID = songId,
                UserID = userId,
                Amount = shareAmount,
                CreatedAt = now
            });
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public const int MaxHistoryEntries = 200;
    private async Task TrimHistoryAsync(int userId)
    {
        var excessRows = await _context.ListeningHistories
            .Where(h => h.UserID == userId)
            .OrderByDescending(h => h.PlayedAt)
            .Skip(MaxHistoryEntries)
            .ToListAsync();
        if (excessRows.Count > 0)
        {
            _context.ListeningHistories.RemoveRange(excessRows);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<List<HistorySongVM>> GetHistoryAsync(int userId, int take = 50)
    {
        var history = await _context.ListeningHistories
            .Where(h => h.UserID == userId)
            .OrderByDescending(h => h.PlayedAt)
            .Take(take)
            .Include(h => h.Song)
                .ThenInclude(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
            .ToListAsync();
        return history
            .Where(h => h.Song != null)
            .Select(h => new HistorySongVM
            {
                SongID = h.Song.SongID,
                Title = h.Song.Title,
                CoverImage = h.Song.CoverImage,
                Duration = h.Song.Duration,
                AudioUrl = h.Song.AudioURL,
                IsPremium = h.Song.IsPremium,
                ArtistNames = string.Join(", ", h.Song.SongArtists.Select(sa => sa.Artist!.ArtistName)),
                MainArtistID = h.Song.SongArtists.Select(sa => (int?)sa.Artist!.ArtistID).FirstOrDefault(),
                PlayedAt = DateTime.SpecifyKind(h.PlayedAt, DateTimeKind.Utc).AddHours(7)
            })
            .ToList();
    }
    public async Task ClearHistoryAsync(int userId)
    {
        var rows = await _context.ListeningHistories
            .Where(h => h.UserID == userId)
            .ToListAsync();
        _context.ListeningHistories.RemoveRange(rows);
        await _context.SaveChangesAsync();
    }
    public async Task<List<TrendingSongVM>> GetTrendingAsync(int take = 10)
    {
        var songs = await _context.Songs
            .Include(s => s.SongStatistic)
            .Include(s => s.SongArtists)
                .ThenInclude(sa => sa.Artist)
            .OrderByDescending(s => s.SongStatistic != null ? s.SongStatistic.TotalPlays : 0)
            .Take(take)
            .ToListAsync();
        return songs.Select(s => new TrendingSongVM
        {
            SongId = s.SongID,
            Title = s.Title,
            CoverImage = s.CoverImage,
            Duration = s.Duration,
            AudioUrl = s.AudioURL,
            Artists = s.SongArtists.Select(sa => sa.Artist!.ArtistName ?? "").ToList()
        }).ToList();
    }
}
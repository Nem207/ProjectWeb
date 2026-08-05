using SpotifyClone.Data;
using Microsoft.EntityFrameworkCore;
using SpotifyClone.Features.AdminDashBoard.ViewModels;
namespace SpotifyClone.Features.AdminDashBoard.Services
{
    public interface DashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
    public class DashboardServiceImpl : DashboardService
    {
        private readonly SpotifyDbContext _context;
        public DashboardServiceImpl(SpotifyDbContext context)
        {
            _context = context;
        }
        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var model = new DashboardViewModel();
            model.TotalUsers = await _context.Users.CountAsync();
            model.TotalSongs = await _context.Songs.CountAsync();
            model.TotalArtists = await _context.Artists.CountAsync();
            model.TotalStreams = await _context.ListeningHistories.LongCountAsync();
            var fromDate = DateTime.Now.Date.AddDays(-6);
            var rawTrend = await _context.ListeningHistories
                .Where(h => h.PlayedAt.Date >= fromDate)
                .GroupBy(h => h.PlayedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();
            model.StreamTrend = Enumerable.Range(0, 7)
                .Select(i => fromDate.AddDays(i))
                .Select(date => new StreamTrendPoint
                {
                    Label = date.ToString("dd/MM"),
                    TotalPlays = rawTrend.FirstOrDefault(r => r.Date == date)?.Count ?? 0
                })
                .ToList();
            var totalSongsForGenre = await _context.Songs.CountAsync();
            var genreGroups = await _context.SongGenres
                .Include(sg => sg.Genre)
                .GroupBy(sg => sg.Genre.GenreName)
                .Select(g => new { GenreName = g.Key, Count = g.Select(x => x.SongID).Distinct().Count() })
                .OrderByDescending(g => g.Count)
                .ToListAsync();
            model.GenreDistribution = genreGroups.Select(g => new GenreShare
            {
                GenreName = string.IsNullOrEmpty(g.GenreName) ? "Chưa phân loại" : g.GenreName,
                SongCount = g.Count,
                Percentage = totalSongsForGenre == 0 ? 0 : Math.Round(g.Count * 100.0 / totalSongsForGenre, 1)
            }).ToList();
            var topRaw = await _context.ListeningHistories
                .GroupBy(h => h.SongID)
                .Select(g => new { SongId = g.Key, StreamCount = g.LongCount() })
                .OrderByDescending(g => g.StreamCount)
                .Take(10)
                .ToListAsync();
            var songIds = topRaw.Select(t => t.SongId).ToList();
            var songs = await _context.Songs
                .Where(s => songIds.Contains(s.SongID))
                .ToListAsync();
            var artistLinks = await _context.SongArtists
                .Include(sa => sa.Artist)
                .Where(sa => songIds.Contains(sa.SongID))
                .ToListAsync();
            model.TopTracks = topRaw.Select(t =>
            {
                var song = songs.FirstOrDefault(s => s.SongID == t.SongId);
                var artistNames = artistLinks
                    .Where(sa => sa.SongID == t.SongId)
                    .Select(sa => sa.Artist.ArtistName);
                return new TopTrack
                {
                    SongId = t.SongId,
                    Title = song?.Title ?? "N/A",
                    ArtistName = artistNames.Any() ? string.Join(", ", artistNames) : "N/A",
                    CoverImageUrl = song?.CoverImage,
                    StreamCount = t.StreamCount
                };
            }).ToList();
            return model;
        }
    }
}
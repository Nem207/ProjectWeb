using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.Album.ViewModels;
using SpotifyClone.Features.Artist.Service;
using SpotifyClone.Features.Artist.ViewModels;
using SpotifyClone.Features.Song.ViewModels;
using SpotifyClone.Models;
namespace SpotifyClone.Features.Artist.Services;

public class ArtistService : IArtistService
{
    private readonly SpotifyDbContext _context;
    public ArtistService(SpotifyDbContext context)
    {
        _context = context;
    }
    public async Task<List<ArtistVM>> GetAllAsync()
    {
        return await _context.Artists
            .Where(a => !a.IsBlocked)
            .Select(a => new ArtistVM { ArtistID = a.ArtistID, ArtistName = a.ArtistName ?? "", Avatar = a.Avatar })
            .ToListAsync();
    }
    public async Task<ArtistVM?> GetByIdAsync(int id)
    {
        return await _context.Artists
            .Where(a => a.ArtistID == id && !a.IsBlocked)
            .Select(a => new ArtistVM { ArtistID = a.ArtistID, ArtistName = a.ArtistName ?? "", Avatar = a.Avatar })
            .FirstOrDefaultAsync();
    }
    public async Task<List<AlbumSummaryVM>> GetAlbumsAsync(int id)
    {
        return await _context.AlbumArtists
            .Where(aa => aa.ArtistID == id && !aa.Artist.IsBlocked)
            .Select(aa => new AlbumSummaryVM
            {
                AlbumID = aa.Album.AlbumID,
                AlbumName = aa.Album.AlbumName ?? "",
                CoverImage = aa.Album.CoverImage,
                ReleaseDate = aa.Album.ReleaseDate,
                AlbumType = aa.Album.AlbumType
            })
            .ToListAsync();
    }
    public async Task<List<SongSummaryVM>> GetSongsAsync(int id)
    {
        return await _context.SongArtists
            .Where(sa => sa.ArtistID == id && sa.Song.Status == SongStatus.Approved && !sa.Artist.IsBlocked)
            .Include(sa => sa.Song).ThenInclude(s => s.SongStatistic)
            .Include(sa => sa.Song).ThenInclude(s => s.SongArtists).ThenInclude(sa2 => sa2.Artist)
            .Select(sa => sa.Song)
            .Select(s => new SongSummaryVM
            {
                SongID = s.SongID,
                AlbumID = s.AlbumID,
                Title = s.Title,
                Duration = s.Duration,
                CoverImage = s.CoverImage,
                AudioURL = s.AudioURL,
                TotalPlays = s.SongStatistic != null ? s.SongStatistic.TotalPlays : 0,
                Artists = s.SongArtists.Select(sa => new ArtistLinkViewModel { ArtistID = sa.ArtistID, ArtistName = sa.Artist.ArtistName ?? "" }).ToList()
            })
            .ToListAsync();
    }
    public async Task<ArtistStatsVM> GetStatsAsync(int id)
    {
        var stats = await _context.ArtistStatistics
            .Where(s => s.ArtistID == id)
            .Select(s => new ArtistStatsVM { MonthlyListeners = s.MonthlyListeners, TotalFollowers = s.TotalFollowers, TotalPlays = s.TotalPlays })
            .FirstOrDefaultAsync();
        return stats ?? new ArtistStatsVM();
    }
    public async Task<ArtistDetailVM?> GetArtistDetailAsync(int id)
    {
        var artist = await _context.Artists
            .Include(a => a.ArtistStatistic)
            .FirstOrDefaultAsync(a => a.ArtistID == id && !a.IsBlocked);
        if (artist == null) return null;
        var popularSongs = await _context.SongArtists
            .Where(sa => sa.ArtistID == id && sa.Song.Status == SongStatus.Approved)
            .Include(sa => sa.Song).ThenInclude(s => s.SongStatistic)
            .Include(sa => sa.Song).ThenInclude(s => s.SongArtists).ThenInclude(sa2 => sa2.Artist)
            .Select(sa => sa.Song)
            .OrderByDescending(s => s.SongStatistic != null ? s.SongStatistic.TotalPlays : 0)
            .Take(10)
            .ToListAsync();
        return new ArtistDetailVM
        {
            ArtistID = artist.ArtistID,
            ArtistName = artist.ArtistName ?? "",
            Avatar = artist.Avatar,
            CoverImage = artist.CoverImage,
            Bio = artist.Bio,
            MonthlyListeners = artist.ArtistStatistic?.MonthlyListeners ?? 0,
            TotalFollowers = artist.ArtistStatistic?.TotalFollowers ?? 0,
            PopularSongs = popularSongs.Select(s => new SongSummaryVM
            {
                SongID = s.SongID,
                AlbumID = s.AlbumID,
                Title = s.Title,
                Duration = s.Duration,
                CoverImage = s.CoverImage,
                AudioURL = s.AudioURL,
                TotalPlays = s.SongStatistic?.TotalPlays ?? 0,
                Artists = s.SongArtists.Select(sa => new ArtistLinkViewModel { ArtistID = sa.ArtistID, ArtistName = sa.Artist.ArtistName ?? "" }).ToList()
            }).ToList()
        };
    }
    public async Task<bool> IsFollowingAsync(int userId, int artistId)
    {
        return await _context.UserFollowArtists
            .AnyAsync(x => x.UserID == userId && x.ArtistID == artistId);
    }
    public async Task<bool> ToggleFollowAsync(int userId, int artistId)
    {
        var existing = await _context.UserFollowArtists
            .FirstOrDefaultAsync(x => x.UserID == userId && x.ArtistID == artistId);
        if (existing != null)
        {
            _context.UserFollowArtists.Remove(existing);
            await _context.SaveChangesAsync();
            return false;
        }
        _context.UserFollowArtists.Add(new UserFollowArtist
        {
            UserID = userId,
            ArtistID = artistId,
            FollowedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<List<ArtistVM>> GetFollowedArtistsAsync(int userId)
    {
        var followRows = await _context.UserFollowArtists
            .Where(x => x.UserID == userId)
            .ToListAsync();
        if (followRows.Count == 0) return new List<ArtistVM>();
        var followedAtByArtistId = followRows.ToDictionary(x => x.ArtistID, x => x.FollowedAt);
        var artistIds = followedAtByArtistId.Keys.ToList();
        var artists = await _context.Artists
            .Where(a => artistIds.Contains(a.ArtistID))
            .ToListAsync();
        return artists
            .OrderByDescending(a => followedAtByArtistId[a.ArtistID])
            .Select(a => new ArtistVM { ArtistID = a.ArtistID, ArtistName = a.ArtistName ?? "", Avatar = a.Avatar ?? "" })
            .ToList();
    }
    public async Task<bool> IsBlockedAsync(int userId, int artistId)
    {
        return await _context.BlockedArtists
            .AnyAsync(x => x.UserID == userId && x.ArtistID == artistId);
    }
    public async Task<bool> ToggleBlockAsync(int userId, int artistId)
    {
        var existing = await _context.BlockedArtists
            .FirstOrDefaultAsync(x => x.UserID == userId && x.ArtistID == artistId);
        if (existing != null)
        {
            _context.BlockedArtists.Remove(existing);
            await _context.SaveChangesAsync();
            return false;
        }
        _context.BlockedArtists.Add(new BlockedArtist
        {
            UserID = userId,
            ArtistID = artistId,
            BlockedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<List<int>> GetBlockedArtistIdsAsync(int userId)
    {
        return await _context.BlockedArtists
            .Where(x => x.UserID == userId)
            .Select(x => x.ArtistID)
            .ToListAsync();
    }
}
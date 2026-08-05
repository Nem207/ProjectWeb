using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.Album.ViewModels;
using SpotifyClone.Features.Song.ViewModels;
using SpotifyClone.Models;
namespace SpotifyClone.Features.Album.Services;
public class AlbumService : IAlbumService
{
    private readonly SpotifyDbContext _context;
    public AlbumService(SpotifyDbContext context)
    {
        _context = context;
    }
    public async Task<List<AlbumVM>> GetAllAsync()
    {
        return await _context.Albums
            .Select(a => new AlbumVM { AlbumID = a.AlbumID, AlbumName = a.AlbumName ?? "", CoverImage = a.CoverImage })
            .ToListAsync();
    }
    public async Task<AlbumVM?> GetByIdAsync(int id)
    {
        return await _context.Albums
            .Where(a => a.AlbumID == id)
            .Select(a => new AlbumVM { AlbumID = a.AlbumID, AlbumName = a.AlbumName ?? "", CoverImage = a.CoverImage })
            .FirstOrDefaultAsync();
    }
    public async Task<List<AlbumArtistLinkVM>> GetArtistsAsync(int id)
    {
        return await _context.AlbumArtists
            .Where(aa => aa.AlbumID == id)
            .Select(aa => new AlbumArtistLinkVM { ArtistID = aa.Artist.ArtistID, ArtistName = aa.Artist.ArtistName ?? "", Avatar = aa.Artist.Avatar })
            .ToListAsync();
    }
    public async Task<List<SongSummaryVM>> GetSongsAsync(int id)
    {
        return await _context.Songs
            .Where(s => s.AlbumID == id && s.Status == SongStatus.Approved)
            .Include(s => s.SongStatistic)
            .Include(s => s.SongArtists).ThenInclude(sa => sa.Artist)
            .Select(s => new SongSummaryVM
            {
                SongID = s.SongID,
                Title = s.Title,
                Duration = s.Duration,
                CoverImage = s.CoverImage,
                AudioURL = s.AudioURL,
                TotalPlays = s.SongStatistic != null ? s.SongStatistic.TotalPlays : 0,
                Artists = s.SongArtists.Select(sa => new ArtistLinkViewModel { ArtistID = sa.ArtistID, ArtistName = sa.Artist.ArtistName ?? "" }).ToList()
            })
            .ToListAsync();
    }
    public async Task<AlbumDetailVM?> GetAlbumDetailAsync(int id)
    {
        var album = await _context.Albums
            .Include(a => a.Songs).ThenInclude(s => s.SongStatistic)
            .Include(a => a.Songs).ThenInclude(s => s.SongArtists).ThenInclude(sa => sa.Artist)
            .Include(a => a.AlbumArtists).ThenInclude(aa => aa.Artist)
            .FirstOrDefaultAsync(a => a.AlbumID == id);
        if (album == null) return null;
        var mainArtist = album.AlbumArtists.Select(aa => aa.Artist).FirstOrDefault();
        var otherAlbums = mainArtist == null
            ? new List<AlbumSummaryVM>()
            : await _context.AlbumArtists
                .Where(aa => aa.ArtistID == mainArtist.ArtistID && aa.AlbumID != id)
                .Select(aa => new AlbumSummaryVM
                {
                    AlbumID = aa.Album.AlbumID,
                    AlbumName = aa.Album.AlbumName ?? "",
                    CoverImage = aa.Album.CoverImage,
                    ReleaseDate = aa.Album.ReleaseDate,
                    AlbumType = aa.Album.AlbumType
                })
                .ToListAsync();
        return new AlbumDetailVM
        {
            AlbumID = album.AlbumID,
            AlbumName = album.AlbumName ?? "",
            CoverImage = album.CoverImage,
            AlbumType = album.AlbumType,
            ReleaseDate = album.ReleaseDate,
            MainArtistID = mainArtist?.ArtistID ?? 0,
            MainArtistName = mainArtist?.ArtistName ?? "",
            MainArtistAvatar = mainArtist?.Avatar,
            Songs = album.Songs.Where(s => s.Status == SongStatus.Approved).Select(s => new AlbumSongVM
            {
                SongID = s.SongID,
                Title = s.Title,
                Duration = s.Duration,
                CoverImage = s.CoverImage,
                AudioURL = s.AudioURL,
                Artists = s.SongArtists.Select(sa => new ArtistLinkViewModel { ArtistID = sa.ArtistID, ArtistName = sa.Artist.ArtistName ?? "" }).ToList()
            }).ToList(),
            OtherAlbums = otherAlbums
        };
    }
}
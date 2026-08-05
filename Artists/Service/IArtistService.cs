using SpotifyClone.Features.Artist.ViewModels;
using SpotifyClone.Features.Album.ViewModels;
using SpotifyClone.Features.Song.ViewModels;
namespace SpotifyClone.Features.Artist.Service;
public interface IArtistService
{
    Task<List<ArtistVM>> GetAllAsync();
    Task<ArtistVM?> GetByIdAsync(int id);
    Task<List<AlbumSummaryVM>> GetAlbumsAsync(int id);
    Task<List<SongSummaryVM>> GetSongsAsync(int id);
    Task<ArtistStatsVM> GetStatsAsync(int id);
    Task<ArtistDetailVM?> GetArtistDetailAsync(int id);
    Task<bool> IsFollowingAsync(int userId, int artistId);
    Task<bool> ToggleFollowAsync(int userId, int artistId);
    Task<List<ArtistVM>> GetFollowedArtistsAsync(int userId);
    Task<bool> IsBlockedAsync(int userId, int artistId);
    Task<bool> ToggleBlockAsync(int userId, int artistId);
    Task<List<int>> GetBlockedArtistIdsAsync(int userId);
}
public class ArtistStatsVM
{
    public long MonthlyListeners { get; set; }
    public long TotalFollowers { get; set; }
    public long TotalPlays { get; set; }
}   
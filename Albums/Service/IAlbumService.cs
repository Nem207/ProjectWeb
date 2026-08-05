using SpotifyClone.Features.Album.ViewModels;
using SpotifyClone.Features.Song.ViewModels;
namespace SpotifyClone.Features.Album.Services;
public interface IAlbumService
{
    Task<List<AlbumVM>> GetAllAsync();
    Task<AlbumVM?> GetByIdAsync(int id);
    Task<List<AlbumArtistLinkVM>> GetArtistsAsync(int id);
    Task<List<SongSummaryVM>> GetSongsAsync(int id);
    Task<AlbumDetailVM?> GetAlbumDetailAsync(int id);
}
public class AlbumArtistLinkVM
{
    public int ArtistID { get; set; }
    public string ArtistName { get; set; } = "";
    public string? Avatar { get; set; }
}
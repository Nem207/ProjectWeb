using SpotifyClone.Features.Song.ViewModels;
namespace SpotifyClone.Features.Playlist.ViewModels;
public class PlaylistDetailVM
{
    public PlaylistVM Playlist { get; set; } = new();
    public List<SongVM> Songs { get; set; } = new();
}
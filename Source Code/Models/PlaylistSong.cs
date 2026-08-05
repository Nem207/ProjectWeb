namespace SpotifyClone.Models;
public class PlaylistSong
{
    public int PlaylistID { get; set; }
    public int SongID { get; set; }
    public DateTime AddedAt { get; set; }
    public Playlist Playlist { get; set; } = null!;
    public Song Song { get; set; } = null!;
}
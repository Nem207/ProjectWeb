namespace SpotifyClone.Models;
public class AlbumArtist
{
    public int AlbumID { get; set; }
    public int ArtistID { get; set; }
    public Album Album { get; set; } = null!;
    public Artist Artist { get; set; } = null!;
}
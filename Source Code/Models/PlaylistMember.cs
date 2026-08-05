namespace SpotifyClone.Models;
public class PlaylistMember
{
    public int PlaylistID { get; set; }
    public int UserID { get; set; }
    public string? RoleName { get; set; }
    public Playlist Playlist { get; set; } = null!;
    public User User { get; set; } = null!;
}
namespace SpotifyClone.Features.Playlist.ViewModels;
public class PlaylistVM
{
    public int PlaylistID { get; set; }
    public string PlaylistName { get; set; } = "";
    public string? CoverImage { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public string OwnerName { get; set; } = "";
    public int SongCount { get; set; }
    public int TotalDurationSeconds { get; set; }
}
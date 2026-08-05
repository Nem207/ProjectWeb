using System.ComponentModel.DataAnnotations;
namespace SpotifyClone.Models;
public class Playlist
{
    [Key]
    public int PlaylistID { get; set; }
    public int? UserID { get; set; }
    public string? PlaylistName { get; set; }
    public string? CoverImage { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public User? User { get; set; }
    public ICollection<PlaylistSong> PlaylistSongs { get; set; }
        = new List<PlaylistSong>();
    public ICollection<PlaylistMember> PlaylistMembers { get; set; }
        = new List<PlaylistMember>();
}
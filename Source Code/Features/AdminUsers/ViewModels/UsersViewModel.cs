namespace SpotifyClone.Features.AdminUsers.ViewModels
{
    public class UsersViewModel
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? AvatarURL { get; set; }
        public bool IsPremium { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

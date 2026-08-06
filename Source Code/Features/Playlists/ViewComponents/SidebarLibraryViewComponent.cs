using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.Artist.Service;
using SpotifyClone.Features.Auth.Services;
using SpotifyClone.Features.Playlist.ViewModels;
namespace SpotifyClone.Features.Playlists.ViewComponents
{
    public class SidebarLibraryViewComponent : ViewComponent
    {
        private readonly SpotifyDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IArtistService _artistService;
        public SidebarLibraryViewComponent(SpotifyDbContext context, ICurrentUserService currentUser, IArtistService artistService)
        {
            _context = context;
            _currentUser = currentUser;
            _artistService = artistService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new SidebarLibraryVM();
            if (_currentUser.IsAuthenticated && _currentUser.UserId is int userId)
            {
                var favoritesName = SpotifyClone.Features.Playlist.Controllers.Api.PlaylistApiController.FavoritesPlaylistName;
                vm.Playlists = await _context.Playlists
                    .Where(p => p.UserID == userId)
                    .OrderByDescending(p => p.PlaylistName == favoritesName)
                    .ThenByDescending(p => p.CreatedAt)
                    .Select(p => new PlaylistVM
                    {
                        PlaylistID = p.PlaylistID,
                        PlaylistName = p.PlaylistName,
                        CoverImage = p.CoverImage,
                        SongCount = p.PlaylistSongs.Count(ps => ps.Song.Status == SpotifyClone.Models.SongStatus.Approved)
                    })
                    .Where(p => p.PlaylistName != favoritesName || p.SongCount > 0)
                    .ToListAsync();
                vm.FollowedArtists = await _artistService.GetFollowedArtistsAsync(userId);
            }
            return View(vm);
        }
    }
    public class SidebarLibraryVM
    {
        public List<PlaylistVM> Playlists { get; set; } = new();
        public List<SpotifyClone.Features.Artist.ViewModels.ArtistVM> FollowedArtists { get; set; } = new();
    }
}
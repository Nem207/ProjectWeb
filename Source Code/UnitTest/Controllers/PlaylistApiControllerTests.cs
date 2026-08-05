using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SpotifyClone.Data;
using SpotifyClone.Features.Playlist.Controllers.Api;
using SpotifyClone.Features.Playlist.ViewModels;
using SpotifyClone.Features.Premium.Services;
using SpotifyClone.Models;
using SpotifyClone.Tests.Helpers;
using Xunit;

namespace SpotifyClone.Tests.Controllers;

public class PlaylistApiControllerTests
{
    private static PlaylistApiController BuildController(
        SpotifyDbContext context,
        FakeCurrentUserService currentUser,
        Mock<IPremiumService>? premiumMock = null)
    {
        premiumMock ??= new Mock<IPremiumService>();
        return new PlaylistApiController(context, currentUser, premiumMock.Object);
    }

    private static async Task SeedSongAsync(SpotifyDbContext context, int songId)
    {
        context.Songs.Add(new Song
        {
            SongID = songId,
            Title = $"Bài {songId}",
            Status = SongStatus.Approved
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetMyPlaylists_ChuaDangNhap_TraVeDanhSachRong()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = BuildController(context, FakeCurrentUserService.Anonymous());

        var result = await controller.GetMyPlaylists();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<PlaylistVM>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Create_ChuaDangNhap_TraVe401()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = BuildController(context, FakeCurrentUserService.Anonymous());

        var result = await controller.Create(new PlaylistVM { PlaylistName = "Test" });

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Create_DaDangNhap_TaoPlaylistThanhCong()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10));

        var result = await controller.Create(new PlaylistVM { PlaylistName = "Playlist của tôi" });

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(context.Playlists);
        Assert.Equal("Playlist của tôi", context.Playlists.First().PlaylistName);
    }

    [Fact]
    public async Task Delete_PlaylistKhongTonTai_TraVe404()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10));

        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddSong_PlaylistKhongTonTai_TraVe404()
    {
        await using var context = TestDbContextFactory.Create();
        await SeedSongAsync(context, 1);
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10));

        var result = await controller.AddSong(playlistId: 999, songId: 1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task AddSong_BaiHatDaCoTrongPlaylist_TraVeAddedFalse_KhongThemTrung()
    {
        await using var context = TestDbContextFactory.Create();
        await SeedSongAsync(context, 1);
        context.Playlists.Add(new Playlist { PlaylistID = 1, UserID = 10, PlaylistName = "P1" });
        context.PlaylistSongs.Add(new PlaylistSong { PlaylistID = 1, SongID = 1, AddedAt = DateTime.Now });
        await context.SaveChangesAsync();
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10));

        var result = await controller.AddSong(1, 1);

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(context.PlaylistSongs); 
    }

    [Fact]
    public async Task AddSong_UserFree_VuotQuaGioiHan5Bai_TraVe403()
    {
        await using var context = TestDbContextFactory.Create();
        context.Playlists.Add(new Playlist { PlaylistID = 1, UserID = 10, PlaylistName = "P1" });
        for (int i = 1; i <= 5; i++)
        {
            await SeedSongAsync(context, i);
            context.PlaylistSongs.Add(new PlaylistSong { PlaylistID = 1, SongID = i, AddedAt = DateTime.Now });
        }
        await SeedSongAsync(context, 6); 
        await context.SaveChangesAsync();

        var premiumMock = new Mock<IPremiumService>();
        premiumMock.Setup(p => p.HasPremiumAsync(10)).ReturnsAsync(false);
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10), premiumMock);

        var result = await controller.AddSong(1, 6);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, statusResult.StatusCode);
        Assert.Equal(5, await context.PlaylistSongs.CountAsync(x => x.PlaylistID == 1));
    }

    [Fact]
    public async Task AddSong_UserPremium_KhongBiGioiHan5Bai()
    {
        await using var context = TestDbContextFactory.Create();
        context.Playlists.Add(new Playlist { PlaylistID = 1, UserID = 10, PlaylistName = "P1" });
        for (int i = 1; i <= 5; i++)
        {
            await SeedSongAsync(context, i);
            context.PlaylistSongs.Add(new PlaylistSong { PlaylistID = 1, SongID = i, AddedAt = DateTime.Now });
        }
        await SeedSongAsync(context, 6);
        await context.SaveChangesAsync();

        var premiumMock = new Mock<IPremiumService>();
        premiumMock.Setup(p => p.HasPremiumAsync(10)).ReturnsAsync(true);
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10), premiumMock);

        var result = await controller.AddSong(1, 6);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(6, await context.PlaylistSongs.CountAsync(x => x.PlaylistID == 1));
    }

    [Fact]
    public async Task AddSong_PlaylistYeuThich_KhongBiGioiHanDuVoiUserFree()
    {
        await using var context = TestDbContextFactory.Create();
        context.Playlists.Add(new Playlist
        {
            PlaylistID = 1,
            UserID = 10,
            PlaylistName = PlaylistApiController.FavoritesPlaylistName
        });
        for (int i = 1; i <= 5; i++)
        {
            await SeedSongAsync(context, i);
            context.PlaylistSongs.Add(new PlaylistSong { PlaylistID = 1, SongID = i, AddedAt = DateTime.Now });
        }
        await SeedSongAsync(context, 6);
        await context.SaveChangesAsync();

        var premiumMock = new Mock<IPremiumService>();
        premiumMock.Setup(p => p.HasPremiumAsync(10)).ReturnsAsync(false);
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10), premiumMock);

        var result = await controller.AddSong(1, 6);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(6, await context.PlaylistSongs.CountAsync(x => x.PlaylistID == 1));
        premiumMock.Verify(p => p.HasPremiumAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RemoveSong_BaiHatKhongTonTaiTrongPlaylist_TraVe404()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10));

        var result = await controller.RemoveSong(1, 1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RemoveSong_TonTai_XoaThanhCong()
    {
        await using var context = TestDbContextFactory.Create();
        context.Playlists.Add(new Playlist { PlaylistID = 1, UserID = 10, PlaylistName = "P1" });
        await SeedSongAsync(context, 1);
        context.PlaylistSongs.Add(new PlaylistSong { PlaylistID = 1, SongID = 1, AddedAt = DateTime.Now });
        await context.SaveChangesAsync();
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10));

        var result = await controller.RemoveSong(1, 1);

        Assert.IsType<OkResult>(result);
        Assert.Empty(context.PlaylistSongs);
    }

    [Fact]
    public async Task GetOrCreateFavoritesPlaylist_ChuaCo_SeTaoMoi()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10));

        var result = await controller.GetOrCreateFavoritesPlaylist();

        Assert.IsType<OkObjectResult>(result);
        var playlist = Assert.Single(context.Playlists);
        Assert.Equal(PlaylistApiController.FavoritesPlaylistName, playlist.PlaylistName);
    }

    [Fact]
    public async Task GetOrCreateFavoritesPlaylist_DaCoRoi_KhongTaoTrung()
    {
        await using var context = TestDbContextFactory.Create();
        context.Playlists.Add(new Playlist
        {
            PlaylistID = 1,
            UserID = 10,
            PlaylistName = PlaylistApiController.FavoritesPlaylistName
        });
        await context.SaveChangesAsync();
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10));

        var result = await controller.GetOrCreateFavoritesPlaylist();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, ok.Value);
        Assert.Single(context.Playlists); 
    }

    [Fact]
    public async Task PeekFavoritesPlaylist_ChuaDangNhap_TraVe401()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = BuildController(context, FakeCurrentUserService.Anonymous());

        var result = await controller.PeekFavoritesPlaylist();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetPlaylist_KhongTonTai_TraVe404()
    {
        await using var context = TestDbContextFactory.Create();
        var controller = BuildController(context, FakeCurrentUserService.Anonymous());

        var result = await controller.GetPlaylist(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ReorderSongs_CapNhatThuTuTheoDanhSachTruyenVao()
    {
        await using var context = TestDbContextFactory.Create();
        context.Playlists.Add(new Playlist { PlaylistID = 1, UserID = 10, PlaylistName = "P1" });
        await SeedSongAsync(context, 1);
        await SeedSongAsync(context, 2);
        context.PlaylistSongs.AddRange(
            new PlaylistSong { PlaylistID = 1, SongID = 1, AddedAt = DateTime.Now },
            new PlaylistSong { PlaylistID = 1, SongID = 2, AddedAt = DateTime.Now.AddSeconds(1) }
        );
        await context.SaveChangesAsync();
        var controller = BuildController(context, FakeCurrentUserService.LoggedIn(10));

        
        var result = await controller.ReorderSongs(1, new PlaylistSongOrderVM { SongIDs = new List<int> { 2, 1 } });

        Assert.IsType<OkResult>(result);
        var song2 = await context.PlaylistSongs.FirstAsync(x => x.SongID == 2);
        var song1 = await context.PlaylistSongs.FirstAsync(x => x.SongID == 1);
        Assert.True(song2.AddedAt < song1.AddedAt);
    }
}

using SpotifyClone.Data;
using SpotifyClone.Features.Artist.Services;
using SpotifyClone.Models;
using SpotifyClone.Tests.Helpers;
using Xunit;

namespace SpotifyClone.Tests.Services;

public class ArtistServiceTests
{
    private static async Task<SpotifyDbContext> SeedBasicArtistsAsync()
    {
        var context = TestDbContextFactory.Create();
        context.Artists.AddRange(
            new Artist { ArtistID = 1, ArtistName = "Sơn Tùng M-TP", IsBlocked = false },
            new Artist { ArtistID = 2, ArtistName = "Bị Chặn", IsBlocked = true }
        );
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task GetAllAsync_ChiTraVe_ArtistKhongBiBlock()
    {
        
        await using var context = await SeedBasicArtistsAsync();
        var service = new ArtistService(context);

        
        var result = await service.GetAllAsync();

        
        Assert.Single(result);
        Assert.Equal("Sơn Tùng M-TP", result[0].ArtistName);
    }

    [Fact]
    public async Task GetByIdAsync_ArtistBiBlock_TraVeNull()
    {
        await using var context = await SeedBasicArtistsAsync();
        var service = new ArtistService(context);

        var result = await service.GetByIdAsync(2);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ArtistTonTaiVaKhongBiBlock_TraVeDungThongTin()
    {
        await using var context = await SeedBasicArtistsAsync();
        var service = new ArtistService(context);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.ArtistID);
        Assert.Equal("Sơn Tùng M-TP", result.ArtistName);
    }

    [Fact]
    public async Task GetByIdAsync_KhongTonTai_TraVeNull()
    {
        await using var context = await SeedBasicArtistsAsync();
        var service = new ArtistService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task ToggleFollowAsync_ChuaFollow_TraVeTrue_VaLuuVaoDb()
    {
        await using var context = await SeedBasicArtistsAsync();
        var service = new ArtistService(context);

        var following = await service.ToggleFollowAsync(userId: 10, artistId: 1);

        Assert.True(following);
        Assert.True(await service.IsFollowingAsync(10, 1));
    }

    [Fact]
    public async Task ToggleFollowAsync_DaFollowRoi_TraVeFalse_VaXoaKhoiDb()
    {
        await using var context = await SeedBasicArtistsAsync();
        var service = new ArtistService(context);
        await service.ToggleFollowAsync(10, 1); 

        var following = await service.ToggleFollowAsync(10, 1); 

        Assert.False(following);
        Assert.False(await service.IsFollowingAsync(10, 1));
    }

    [Fact]
    public async Task GetFollowedArtistsAsync_TraVeDanhSachTheoThuTuMoiFollowTruoc()
    {
        await using var context = await SeedBasicArtistsAsync();
        context.Artists.Add(new Artist { ArtistID = 3, ArtistName = "Artist Ba", IsBlocked = false });
        await context.SaveChangesAsync();
        var service = new ArtistService(context);

        await service.ToggleFollowAsync(10, 1);
        await Task.Delay(5); 
        await service.ToggleFollowAsync(10, 3);

        var result = await service.GetFollowedArtistsAsync(10);

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].ArtistID); 
        Assert.Equal(1, result[1].ArtistID);
    }

    [Fact]
    public async Task GetFollowedArtistsAsync_KhongCoAiDuocFollow_TraVeDanhSachRong()
    {
        await using var context = await SeedBasicArtistsAsync();
        var service = new ArtistService(context);

        var result = await service.GetFollowedArtistsAsync(999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ToggleBlockAsync_ChuaBlock_TraVeTrue()
    {
        await using var context = await SeedBasicArtistsAsync();
        var service = new ArtistService(context);

        var blocked = await service.ToggleBlockAsync(userId: 10, artistId: 1);

        Assert.True(blocked);
        Assert.Contains(1, await service.GetBlockedArtistIdsAsync(10));
    }

    [Fact]
    public async Task ToggleBlockAsync_DaBlockRoi_TraVeFalse()
    {
        await using var context = await SeedBasicArtistsAsync();
        var service = new ArtistService(context);
        await service.ToggleBlockAsync(10, 1);

        var blocked = await service.ToggleBlockAsync(10, 1);

        Assert.False(blocked);
        Assert.DoesNotContain(1, await service.GetBlockedArtistIdsAsync(10));
    }

    [Fact]
    public async Task GetStatsAsync_KhongCoDuLieuThongKe_TraVeStatsRong()
    {
        await using var context = await SeedBasicArtistsAsync();
        var service = new ArtistService(context);

        var stats = await service.GetStatsAsync(1);

        Assert.NotNull(stats);
        Assert.Equal(0, stats.MonthlyListeners);
        Assert.Equal(0, stats.TotalFollowers);
        Assert.Equal(0, stats.TotalPlays);
    }

    [Fact]
    public async Task GetStatsAsync_CoDuLieu_TraVeDungGiaTri()
    {
        await using var context = await SeedBasicArtistsAsync();
        context.ArtistStatistics.Add(new ArtistStatistic
        {
            ArtistID = 1,
            MonthlyListeners = 1000,
            TotalFollowers = 200,
            TotalPlays = 5000
        });
        await context.SaveChangesAsync();
        var service = new ArtistService(context);

        var stats = await service.GetStatsAsync(1);

        Assert.Equal(1000, stats.MonthlyListeners);
        Assert.Equal(200, stats.TotalFollowers);
        Assert.Equal(5000, stats.TotalPlays);
    }

    [Fact]
    public async Task GetAlbumsAsync_ArtistBiBlock_KhongTraVeAlbumNao()
    {
        await using var context = await SeedBasicArtistsAsync();
        var album = new Album { AlbumID = 1, AlbumName = "Album X" };
        context.Albums.Add(album);
        context.AlbumArtists.Add(new AlbumArtist { AlbumID = 1, ArtistID = 2 }); 
        await context.SaveChangesAsync();
        var service = new ArtistService(context);

        var result = await service.GetAlbumsAsync(2);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSongsAsync_ChiTraVeBaiHatDaDuocDuyet()
    {
        await using var context = await SeedBasicArtistsAsync();
        var songApproved = new Song { SongID = 1, Title = "Bài đã duyệt", Status = SongStatus.Approved };
        var songPending = new Song { SongID = 2, Title = "Bài chờ duyệt", Status = SongStatus.Pending };
        context.Songs.AddRange(songApproved, songPending);
        context.SongArtists.AddRange(
            new SongArtist { SongID = 1, ArtistID = 1 },
            new SongArtist { SongID = 2, ArtistID = 1 }
        );
        await context.SaveChangesAsync();
        var service = new ArtistService(context);

        var result = await service.GetSongsAsync(1);

        Assert.Single(result);
        Assert.Equal("Bài đã duyệt", result[0].Title);
    }
}

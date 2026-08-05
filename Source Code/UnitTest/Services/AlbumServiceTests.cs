using SpotifyClone.Features.Album.Services;
using SpotifyClone.Models;
using SpotifyClone.Tests.Helpers;
using Xunit;

namespace SpotifyClone.Tests.Services;

public class AlbumServiceTests
{
    [Fact]
    public async Task GetAllAsync_TraVeTatCaAlbum()
    {
        await using var context = TestDbContextFactory.Create();
        context.Albums.AddRange(
            new Album { AlbumID = 1, AlbumName = "Album A" },
            new Album { AlbumID = 2, AlbumName = "Album B" }
        );
        await context.SaveChangesAsync();
        var service = new AlbumService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_KhongTonTai_TraVeNull()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new AlbumService(context);

        var result = await service.GetByIdAsync(123);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_TonTai_TraVeDungAlbum()
    {
        await using var context = TestDbContextFactory.Create();
        context.Albums.Add(new Album { AlbumID = 1, AlbumName = "Sky Tour" });
        await context.SaveChangesAsync();
        var service = new AlbumService(context);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Sky Tour", result!.AlbumName);
    }

    [Fact]
    public async Task GetArtistsAsync_TraVeDungDanhSachNgheSiCuaAlbum()
    {
        await using var context = TestDbContextFactory.Create();
        context.Albums.Add(new Album { AlbumID = 1, AlbumName = "Album A" });
        context.Artists.AddRange(
            new Artist { ArtistID = 1, ArtistName = "Artist 1" },
            new Artist { ArtistID = 2, ArtistName = "Artist 2" }
        );
        context.AlbumArtists.Add(new AlbumArtist { AlbumID = 1, ArtistID = 1 });
        await context.SaveChangesAsync();
        var service = new AlbumService(context);

        var result = await service.GetArtistsAsync(1);

        Assert.Single(result);
        Assert.Equal("Artist 1", result[0].ArtistName);
    }

    [Fact]
    public async Task GetSongsAsync_ChiTraVeBaiHatDaDuyet_CuaDungAlbum()
    {
        await using var context = TestDbContextFactory.Create();
        context.Albums.Add(new Album { AlbumID = 1, AlbumName = "Album A" });
        context.Songs.AddRange(
            new Song { SongID = 1, AlbumID = 1, Title = "Bài 1", Status = SongStatus.Approved },
            new Song { SongID = 2, AlbumID = 1, Title = "Bài 2 (pending)", Status = SongStatus.Pending },
            new Song { SongID = 3, AlbumID = 2, Title = "Bài của album khác", Status = SongStatus.Approved }
        );
        await context.SaveChangesAsync();
        var service = new AlbumService(context);

        var result = await service.GetSongsAsync(1);

        Assert.Single(result);
        Assert.Equal("Bài 1", result[0].Title);
    }

    [Fact]
    public async Task GetAlbumDetailAsync_KhongTonTai_TraVeNull()
    {
        await using var context = TestDbContextFactory.Create();
        var service = new AlbumService(context);

        var result = await service.GetAlbumDetailAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAlbumDetailAsync_TraVeThongTinDayDu_KemAlbumKhacCuaCungNgheSi()
    {
        await using var context = TestDbContextFactory.Create();
        var artist = new Artist { ArtistID = 1, ArtistName = "Main Artist" };
        var album1 = new Album { AlbumID = 1, AlbumName = "Album Chinh" };
        var album2 = new Album { AlbumID = 2, AlbumName = "Album Khac" };
        context.Artists.Add(artist);
        context.Albums.AddRange(album1, album2);
        context.AlbumArtists.AddRange(
            new AlbumArtist { AlbumID = 1, ArtistID = 1 },
            new AlbumArtist { AlbumID = 2, ArtistID = 1 }
        );
        context.Songs.Add(new Song
        {
            SongID = 1,
            AlbumID = 1,
            Title = "Bài trong album chính",
            Status = SongStatus.Approved
        });
        await context.SaveChangesAsync();
        var service = new AlbumService(context);

        var result = await service.GetAlbumDetailAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Album Chinh", result!.AlbumName);
        Assert.Equal("Main Artist", result.MainArtistName);
        Assert.Single(result.Songs);
        Assert.Single(result.OtherAlbums);
        Assert.Equal("Album Khac", result.OtherAlbums[0].AlbumName);
    }
}

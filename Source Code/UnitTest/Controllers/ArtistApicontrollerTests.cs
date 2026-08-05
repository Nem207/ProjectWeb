using Microsoft.AspNetCore.Mvc;
using Moq;
using SpotifyClone.Features.Artist.Controllers.Api;
using SpotifyClone.Features.Artist.Service;
using SpotifyClone.Features.Artist.ViewModels;
using SpotifyClone.Tests.Helpers;
using Xunit;

namespace SpotifyClone.Tests.Controllers;

public class ArtistApicontrollerTests
{
    private static ArtistApicontroller BuildController(
        Mock<IArtistService> serviceMock,
        FakeCurrentUserService currentUser)
    {
        return new ArtistApicontroller(serviceMock.Object, currentUser);
    }

    [Fact]
    public async Task GetById_KhongTonTai_TraVe404()
    {
        var serviceMock = new Mock<IArtistService>();
        serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ArtistVM?)null);
        var controller = BuildController(serviceMock, FakeCurrentUserService.Anonymous());

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_TonTai_TraVe200_KemThongTinArtist()
    {
        var serviceMock = new Mock<IArtistService>();
        var artist = new ArtistVM { ArtistID = 1, ArtistName = "Test Artist" };
        serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(artist);
        var controller = BuildController(serviceMock, FakeCurrentUserService.Anonymous());

        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(artist, okResult.Value);
    }

    [Fact]
    public async Task ToggleFollow_ChuaDangNhap_TraVe401()
    {
        var serviceMock = new Mock<IArtistService>();
        var controller = BuildController(serviceMock, FakeCurrentUserService.Anonymous());

        var result = await controller.ToggleFollow(1);

        Assert.IsType<UnauthorizedObjectResult>(result);
        serviceMock.Verify(s => s.ToggleFollowAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ToggleFollow_DaDangNhap_GoiServiceVaTraVeKetQua()
    {
        var serviceMock = new Mock<IArtistService>();
        serviceMock.Setup(s => s.ToggleFollowAsync(10, 1)).ReturnsAsync(true);
        var controller = BuildController(serviceMock, FakeCurrentUserService.LoggedIn(10));

        var result = await controller.ToggleFollow(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        serviceMock.Verify(s => s.ToggleFollowAsync(10, 1), Times.Once);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task IsFollowing_ChuaDangNhap_TraVeFalse_KhongGoiService()
    {
        var serviceMock = new Mock<IArtistService>();
        var controller = BuildController(serviceMock, FakeCurrentUserService.Anonymous());

        var result = await controller.IsFollowing(1);

        Assert.IsType<OkObjectResult>(result);
        serviceMock.Verify(s => s.IsFollowingAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ToggleBlock_ChuaDangNhap_TraVe401()
    {
        var serviceMock = new Mock<IArtistService>();
        var controller = BuildController(serviceMock, FakeCurrentUserService.Anonymous());

        var result = await controller.ToggleBlock(1);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetFollowed_ChuaDangNhap_TraVeDanhSachRong()
    {
        var serviceMock = new Mock<IArtistService>();
        var controller = BuildController(serviceMock, FakeCurrentUserService.Anonymous());

        var result = await controller.GetFollowed();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<object>>(okResult.Value);
        Assert.Empty(list);
        serviceMock.Verify(s => s.GetFollowedArtistsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAll_TraVeDanhSachTuService()
    {
        var serviceMock = new Mock<IArtistService>();
        var list = new List<ArtistVM> { new() { ArtistID = 1, ArtistName = "A" } };
        serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(list);
        var controller = BuildController(serviceMock, FakeCurrentUserService.Anonymous());

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(list, okResult.Value);
    }
}

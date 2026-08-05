using SpotifyClone.Features.Auth.Services;

namespace SpotifyClone.Tests.Helpers;


public class FakeCurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated { get; set; }
    public int? UserId { get; set; }

    public static FakeCurrentUserService LoggedIn(int userId) =>
        new() { IsAuthenticated = true, UserId = userId };

    public static FakeCurrentUserService Anonymous() =>
        new() { IsAuthenticated = false, UserId = null };
}

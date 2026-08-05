using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;

namespace SpotifyClone.Tests.Helpers;


public static class TestDbContextFactory
{
    public static SpotifyDbContext Create()
    {
        var options = new DbContextOptionsBuilder<SpotifyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new SpotifyDbContext(options);
    }
}

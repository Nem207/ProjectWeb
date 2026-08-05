using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.Album.Services;
using SpotifyClone.Features.Artist.Service;
using SpotifyClone.Features.Artist.Services;
using SpotifyClone.Features.Premium.Services;
using SpotifyClone.Features.Search.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using SpotifyClone.Features.Auth.Services;
using SpotifyClone.Features.AdminAlbums.Services;
using SpotifyClone.Features.AdminArtists.Services;
using SpotifyClone.Features.AdminDashBoard.Services;
using SpotifyClone.Features.AdminSongApproval.Services;
using SpotifyClone.Features.AdminSongs.Services;
using SpotifyClone.Features.AdminUsers.Services;
using SpotifyClone.Features.Home.Services;
using SpotifyClone.Features.Queue.Services;
using SpotifyClone.Features.Song.Services;
using SpotifyClone.Features.Profile.Services;
using SpotifyClone.Features.SongReports.Services;
using SpotifyClone.Features.MusicPlayer.Services;
using SpotifyClone.Features.ArtistReports.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddDbContext<SpotifyDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IPremiumService, PremiumService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<SpotifyClone.Features.ArtistDashboard.Services.IArtistDashboardService, SpotifyClone.Features.ArtistDashboard.Services.ArtistDashboardService>();
builder.Services.AddScoped<IQueueService, QueueService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<SpotifyClone.Features.LikedSongs.Services.ILikedSongsService, SpotifyClone.Features.LikedSongs.Services.LikedSongsService>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IArtistReportService, ArtistReportService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<SongApprovalService>();
builder.Services.AddScoped<SongsService>();
builder.Services.AddScoped<ArtistsService>();
builder.Services.AddScoped<AlbumsService>();
builder.Services.AddScoped<DashboardService, DashboardServiceImpl>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ISongReportService, SongReportService>();
builder.Services.AddScoped<IMusicPlayerService, MusicPlayerService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
builder.Services.AddScoped<IEmailSender>(sp =>
{
    var smtpHost = builder.Configuration["Smtp:Host"];
    if (string.IsNullOrWhiteSpace(smtpHost))
    {
        return new ConsoleEmailSender(sp.GetRequiredService<ILogger<ConsoleEmailSender>>());
    }
    return new SmtpEmailSender(builder.Configuration);
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
foreach (var endpoint in app.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>().Endpoints)
{
    Console.WriteLine(endpoint.DisplayName);
}
app.Run();
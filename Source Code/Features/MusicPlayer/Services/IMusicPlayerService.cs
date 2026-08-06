using SpotifyClone.Features.MusicPlayer.ViewModels;
namespace SpotifyClone.Features.MusicPlayer.Services
{
    public interface IMusicPlayerService
    {
        Task<NowPlayingVM?> GetSongAsync(int songId, int? userId);
        Task<bool> IncrementPlayCountAsync(int songId, int? userId);
        Task<bool> RegisterListenEarningAsync(int songId, int? userId);
        Task<List<TrendingSongVM>> GetTrendingAsync(int take = 10);
        Task<List<HistorySongVM>> GetHistoryAsync(int userId, int take = 50);
        Task ClearHistoryAsync(int userId);
    }
}
namespace SpotifyClone.Helpers
{
    public static class DurationFormatter
    {
        public static string ToMinutesSeconds(int totalSeconds)
        {
            if (totalSeconds < 0) totalSeconds = 0;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes}:{seconds:D2}";
        }
    }
}
namespace MinersWatch
{
    /// <summary>
    /// Game difficulty settings — persisted via PlayerPrefs.
    /// Used by WaveManager to scale enemy count.
    /// </summary>
    public static class GameSettings
    {
        public enum Difficulty { Easy, Normal, Hard }

        private const string Key = "game_difficulty";

        public static Difficulty Current
        {
            get => (Difficulty)UnityEngine.PlayerPrefs.GetInt(Key, 1);
            set => UnityEngine.PlayerPrefs.SetInt(Key, (int)value);
        }

        public static float EnemyCountMultiplier => Current switch
        {
            Difficulty.Easy => 0.6f,
            Difficulty.Normal => 1.0f,
            Difficulty.Hard => 1.5f,
            _ => 1.0f,
        };

        public static float EnemyDamageMultiplier => Current switch
        {
            Difficulty.Easy => 0.7f,
            Difficulty.Normal => 1.0f,
            Difficulty.Hard => 1.4f,
            _ => 1.0f,
        };

        public static string DifficultyLabel(Difficulty d) => d switch
        {
            Difficulty.Easy => "简单",
            Difficulty.Normal => "普通",
            Difficulty.Hard => "困难",
            _ => "普通",
        };
    }
}

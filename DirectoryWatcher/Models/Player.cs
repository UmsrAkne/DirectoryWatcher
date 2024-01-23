using System;
using System.Media;
using System.Threading.Tasks;

namespace DirectoryWatcher.Models
{
    public static class Player
    {
        public static async Task PlaySoundAsync(string filePath)
        {
            try
            {
                // SoundPlayer インスタンスを作成してサウンドファイルを読み込む
                using var soundPlayer = new SoundPlayer(filePath);
                await Task.Run(() => soundPlayer.PlaySync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
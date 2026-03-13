using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using YoutubeExplode;
using YoutubeExplode.Converter;
using Ookii.Dialogs.Wpf;

namespace YouTube_Downloader
{
    public partial class MainWindow : Window
    {

        YoutubeClient youtube = new YoutubeClient();

        List<string> videoUrls = new List<string>();

        private Progress progressWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {

            if (URL.Text == "")
                return;

            string url = URL.Text;

            videoUrls.Add(url);
            VideoList.Items.Add("Cím lekérése...");

            int index = VideoList.Items.Count - 1;

            URL.Clear();

            _ = LoadVideoTitle(url, index);

        }

        async Task LoadVideoTitle(string url, int index)
        {

            try
            {

                var video = await youtube.Videos.GetAsync(url);

                Dispatcher.Invoke(() =>
                {
                    VideoList.Items[index] = video.Title;
                });

            }
            catch
            {

                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("A megadott link érvénytelen.", "Hiba",MessageBoxButton.OK, MessageBoxImage.Error);
                    VideoList.Items.RemoveAt(index);
                    videoUrls.RemoveAt(index);
                });

            }

        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {

            if (VideoList.SelectedIndex < 0)
                return;

            int count = VideoList.Items.Count;
            for (int i = 0; i < count; i++)
            {
                VideoList.Items.RemoveAt(0);
                videoUrls.RemoveAt(0);
            }
        }

        private void DeleteOne_Click(object sender, RoutedEventArgs e)
        {
            if (VideoList.SelectedIndex < 0)
                return;

            int i = VideoList.SelectedIndex;
            VideoList.Items.RemoveAt(i);
            videoUrls.RemoveAt(i);
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new Settings();
            settingsWindow.Owner = this;
            if (settingsWindow.ShowDialog() != true)
                return;

            string selectedFolder = settingsWindow.SelectedFolder;
            bool isMp3 = settingsWindow.IsMp3;

            if (videoUrls.Count == 0)
            {
                MessageBox.Show("A letöltési lista üres.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!FfmpegExists())
            {
                MessageBox.Show(
                    "Nem található a ffmpeg.exe. Töltse le az ffmpeg-et (https://ffmpeg.org) és helyezze az alkalmazás mellé.",
                    "Hiányzó ffmpeg", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            progressWindow = new Progress();
            progressWindow.Owner = this;
            Hide();
            progressWindow.Show();

            await DownloadAll(selectedFolder, isMp3);

            progressWindow.Close();

            int count = VideoList.Items.Count;
            for (int i = 0; i < count; i++)
            {
                VideoList.Items.RemoveAt(0);
                videoUrls.RemoveAt(0);
            }

            Show();
            MessageBox.Show("A letöltés sikeresen befejeződött.", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task DownloadAll(string folder, bool mp3)
        {
            progressWindow.SetTotalProgress(0);

            int index = 0;
            foreach (var url in videoUrls)
            {
                await DownloadVideo(url, index, folder, mp3);
                index++;
            }

            progressWindow.SetStatus("Kész");
        }

        private async Task DownloadVideo(string url, int index, string folder, bool mp3)
        {
            var video = await youtube.Videos.GetAsync(url);

            progressWindow.SetStatus("Letöltés: " + video.Title);

            string safeTitle = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));
            string ext = mp3 ? ".mp3" : ".mp4";
            string path = Path.Combine(folder, safeTitle + ext);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var progress = new Progress<double>(p =>
            {
                progressWindow.SetVideoProgress(p * 100);

                double total =
                    ((double)index / videoUrls.Count) * 100
                    + (p * (100.0 / videoUrls.Count));

                progressWindow.SetTotalProgress(total);

                double seconds = sw.Elapsed.TotalSeconds;
                if (seconds > 0)
                {
                    double totalProgressPercent = ((double)index / videoUrls.Count) * 100 + (p * (100.0 / videoUrls.Count));
                    
                    if (totalProgressPercent > 0)
                    {
                        double totalEstimatedTime = seconds * 100 / totalProgressPercent;
                        double remainingTime = totalEstimatedTime - seconds;
                        
                        progressWindow.SetTimeLeft("Hátralévő idő: " + remainingTime.ToString("0") + " s");
                    }
                }
            });

            try
            {
                await youtube.Videos.DownloadAsync(url, path, progress);
            }
            catch (System.ComponentModel.Win32Exception winEx)
            {
                MessageBox.Show("Nem sikerült elindítani az ffmpeg folyamatot: " + winEx.Message + "\nEllenőrizze, hogy az ffmpeg.exe az alkalmazás mellett legyen", "Ffmpeg hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Letöltési hiba: " + ex.Message, ex);
            }
        }

        private bool FfmpegExists()
        {
            const string exeName = "ffmpeg.exe";

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
                var candidate = Path.Combine(baseDir, exeName);
                if (File.Exists(candidate))
                    return true;
            }
            catch { }

            try
            {
                var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                var paths = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in paths)
                {
                    try
                    {
                        string trimmed = p.Trim();
                        if (trimmed.Length == 0) continue;
                        var candidate = Path.Combine(trimmed, exeName);
                        if (File.Exists(candidate))
                            return true;
                    }
                    catch { }
                }
            }
            catch { }

            return false;
        }
    }
}
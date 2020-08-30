using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Reflection;
using System.IO;
using NYoutubeDL;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using System.Text.RegularExpressions;
using System.Xaml;
using System.Windows.Automation.Peers;

namespace YTDownloadConvert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void bGo_Click(object sender, RoutedEventArgs e)
        {
            string runningPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var downloader = new YoutubeDL();
            string fileName = "";
            downloader.VideoUrl = tbLink.Text;
            downloader.Options.FilesystemOptions.NoMtime = true;
            //downloader.Options.VideoFormatOptions.Format = NYoutubeDL.Helpers.Enums.VideoFormat.best;
            downloader.Options.VerbositySimulationOptions.CallHome = false;
            string videoTitle = downloader.GetDownloadInfo().Title;
            Random random = new Random();
            int randomInt = random.Next(1, 1000);
            downloader.Options.FilesystemOptions.Output = runningPath + $"\\video{randomInt}";
            if (cboxCut.IsChecked == false && cboxConvert.IsChecked == false)
                downloader.Options.FilesystemOptions.Output = runningPath + $"\\{videoTitle}";
            downloader.YoutubeDlPath = runningPath + "\\youtube-dl.exe";

            lDownloading.Visibility = Visibility.Visible;
            await downloader.DownloadAsync();
            foreach (string file in Directory.GetFiles(runningPath))
                if (Path.GetFileName(file).StartsWith($"video{randomInt}"))
                    fileName = file;

            if (cboxCut.IsChecked == true || cboxConvert.IsChecked == true)
            {
                lDownloading.Content = "Getting FFmpeg";
                if (!File.Exists(runningPath + "\\ffmpeg.exe"))
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Full, runningPath);
                lDownloading.Content = "Cutting";
                var conversions = FFmpeg.Conversions.New();
                if (cboxConvert.IsChecked == true && cboxCut.IsChecked == false)
                    await conversions.AddParameter($"-i \"{fileName}\" \"{runningPath + $"\\{videoTitle}{cbConvertType.Text}"}\"").Start();
                if (cboxConvert.IsChecked == true && cboxCut.IsChecked == true)
                    await conversions.AddParameter($"-ss {TimeSpan.FromSeconds(Int32.Parse(tbFrom.Text))} -i \"{fileName}\" -to {TimeSpan.FromSeconds(Int32.Parse(tbTo.Text) - Int32.Parse(tbFrom.Text))} -c copy \"{runningPath + $"\\{videoTitle}{cbConvertType.Text}"}\"").Start();
                if (cboxConvert.IsChecked == false && cboxCut.IsChecked == true)
                    await conversions.AddParameter($"-ss {TimeSpan.FromSeconds(Int32.Parse(tbFrom.Text))} -i \"{fileName}\" -to {TimeSpan.FromSeconds(Int32.Parse(tbTo.Text) - Int32.Parse(tbFrom.Text))} -c copy \"{runningPath + $"\\{videoTitle}{Path.GetExtension(fileName)}"}\"").Start();
                File.Delete(fileName);
                lDownloading.Content = "Downloading";
            }
            lDownloading.Visibility = Visibility.Hidden;
        }

        private void tbNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tbInput = (TextBox)sender;
            Regex regexObj = new Regex(@"[^\d]");
            tbInput.Text = regexObj.Replace(tbInput.Text, "");
            if (tbInput.Text.Length > 4)
                tbInput.Text = tbInput.Text.Remove(4);
            //if (Int32.Parse(tbFrom.Text) > Int32.Parse(tbTo.Text))
            //    tbTo.Text = tbFrom.Text;
        }
    }
}

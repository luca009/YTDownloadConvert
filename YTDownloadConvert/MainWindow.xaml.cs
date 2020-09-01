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
using NYoutubeDL.Helpers;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using System.Text.RegularExpressions;
using System.Xaml;
using System.Windows.Automation.Peers;
using System.Windows.Threading;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace YTDownloadConvert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string runningPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string filePath;
        YoutubeDL downloader = new YoutubeDL();

        public MainWindow()
        {
            InitializeComponent();

            if (Properties.Settings.Default.defaultFilePath != "")
                filePath = Properties.Settings.Default.defaultFilePath;
            else
                filePath = runningPath;
            tbDownloadPath.Text = filePath;
        }

        private async void bGo_Click(object sender, RoutedEventArgs e)
        {
            textStatus.Text = "Preparing";
            bGo.IsEnabled = false;
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.Render);
            downloader = new YoutubeDL();
            string fileName = "";
            downloader.VideoUrl = tbLink.Text;
            downloader.Options.FilesystemOptions.NoMtime = true;
            //downloader.Options.VideoFormatOptions.Format = NYoutubeDL.Helpers.Enums.VideoFormat.best;
            downloader.Options.VerbositySimulationOptions.CallHome = false;
            downloader.Options.GeneralOptions.Update = true;
            //downloader.Options.VideoFormatOptions.FormatAdvanced = 
            string videoTitleNoFilter = downloader.GetDownloadInfo().Title;
            string videoTitle = Regex.Replace(videoTitleNoFilter, @"[^a-z,0-9, ,-]", "", RegexOptions.IgnoreCase);
            Random random = new Random();
            int randomInt = random.Next(1, 1000);
            downloader.Options.FilesystemOptions.Output = filePath + $"\\video{randomInt}";
            if (cboxCut.IsChecked == false && cboxConvert.IsChecked == false)
                downloader.Options.FilesystemOptions.Output = filePath + $"\\{videoTitle}";
            downloader.YoutubeDlPath = runningPath + "\\youtube-dl.exe";
            if (rbBest.IsChecked == true)
                downloader.Options.VideoFormatOptions.FormatAdvanced = "best";
            else
                downloader.Options.VideoFormatOptions.FormatAdvanced = "worst";
            if (cboxResolution.Text != "Auto")
                downloader.Options.VideoFormatOptions.FormatAdvanced += $"[height <=? {Int32.Parse(cboxResolution.Text.TrimEnd('p'))}]";

            textStatus.Text = "Downloading";
            pbProgress.Visibility = Visibility.Visible;
            downloader.DownloadAsync();
            while (downloader.IsDownloading)
            {
                pbProgress.Value = downloader.Info.VideoProgress;
                textETA.Text = "ETA: " + downloader.Info.Eta;
                textSpeed.Text = "Speed: " + downloader.Info.DownloadRate;
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.Render);
            }
            pbProgress.Value = 0;
            textETA.Text = "ETA: ?";
            textSpeed.Text = "Speed: ?";
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.Render);
            foreach (string file in Directory.GetFiles(filePath))
                if (Path.GetFileName(file).StartsWith($"video{randomInt}"))
                    fileName = file;

            if (cboxCut.IsChecked == true || cboxConvert.IsChecked == true || cboxBitrate.IsChecked == true)
            {
                pbProgress.IsIndeterminate = true;
                textStatus.Text = "Getting FFmpeg";
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.Render);
                if (!File.Exists(runningPath + "\\ffmpeg.exe"))
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Full, runningPath);
                textStatus.Text = "Processing";
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.Render);
                var conversions = FFmpeg.Conversions.New();
                if (cboxCut.IsChecked == true)
                    conversions.AddParameter($"-ss {TimeSpan.FromSeconds(Int32.Parse(tbFrom.Text))}");
                conversions.AddParameter($"-i \"{fileName}\"");
                if (cboxConvert.IsChecked == true)
                    conversions.SetOutput($"{filePath}\\{videoTitle}{cbConvertType.Text}");
                else
                    conversions.SetOutput($"{filePath}\\{videoTitle}{Path.GetExtension(fileName)}");
                if (cboxCut.IsChecked == true)
                    conversions.AddParameter($"-to {TimeSpan.FromSeconds(Int32.Parse(tbTo.Text) - Int32.Parse(tbFrom.Text))}");
                if (cboxBitrate.IsChecked == true)
                    conversions.AddParameter($"-b {tbBitrate.Text}k");
                /*
                if (cboxConvert.IsChecked == true && cboxCut.IsChecked == false)
                    await conversions.AddParameter($"-i \"{fileName}\" \"{filePath}\\{videoTitle}{cbConvertType.Text}\"").Start();
                if (cboxConvert.IsChecked == true && cboxCut.IsChecked == true)
                    await conversions.AddParameter($"-ss {TimeSpan.FromSeconds(Int32.Parse(tbFrom.Text))} -i \"{fileName}\" -to {TimeSpan.FromSeconds(Int32.Parse(tbTo.Text) - Int32.Parse(tbFrom.Text))} -c copy \"{filePath + $"\\{videoTitle}{cbConvertType.Text}"}\"").Start();
                if (cboxConvert.IsChecked == false && cboxCut.IsChecked == true)
                    await conversions.AddParameter($"-ss {TimeSpan.FromSeconds(Int32.Parse(tbFrom.Text))} -i \"{fileName}\" -to {TimeSpan.FromSeconds(Int32.Parse(tbTo.Text) - Int32.Parse(tbFrom.Text))} -c copy \"{filePath + $"\\{videoTitle}{Path.GetExtension(fileName)}"}\"").Start();*/
                await conversions.Start();
                File.Delete(fileName);
                pbProgress.IsIndeterminate = false;
            }
            textStatus.Text = "Idle";
            pbProgress.Visibility = Visibility.Hidden;
            bGo.IsEnabled = true;
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

        private void bSet_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog fbdFolder = new VistaFolderBrowserDialog();
            if (fbdFolder.ShowDialog() == true)
            {
                tbDownloadPath.Text = fbdFolder.SelectedPath;
                filePath = fbdFolder.SelectedPath;
                if (MessageBox.Show("Set this path as the default?", "Default?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Properties.Settings.Default.defaultFilePath = fbdFolder.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }
    }
}

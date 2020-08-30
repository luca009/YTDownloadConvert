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
using System.Windows.Shapes;
using System.Reflection;
using System.IO;
using NYoutubeDL;
using Xabe.FFmpeg;

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
            var downloader = new YoutubeDL();
            //downloader.Options.PostProcessingOptions;
            downloader.Options.FilesystemOptions.Output = "C:\\Users\\reall\\Downloads\\video.mp4";
            downloader.YoutubeDlPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\youtube-dl.exe";

            lDownloading.Visibility = Visibility.Visible;

            await downloader.DownloadAsync(tbLink.Text);

            lDownloading.Visibility = Visibility.Hidden;
        }
    }
}

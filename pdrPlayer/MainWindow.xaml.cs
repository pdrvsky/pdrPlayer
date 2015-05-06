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
using Ookii.Dialogs.Wpf;
using System.Windows.Resources;

namespace pdrPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PlaybackControl playback;
        bool playlistHidden = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void playPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (playback == null)
            {
                VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        playback = new PlaybackControl(dialog.SelectedPath);
                        playback.TrackChanged += new PlaybackControl.TrackChangedHandler(changeArtwork);
                        playback.TrackChanged += new PlaybackControl.TrackChangedHandler(updateInterface);
                        changeArtwork();
                        this.updateInterface();
                    }
                    catch (Exception exception) {
                        Console.WriteLine(exception.Message);
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (playback.PLAYBACK_STATE == PlaybackControl.PLAYBACK_STATE_PLAYING) playback.Pause();
                else if (playback.PLAYBACK_STATE == PlaybackControl.PLAYBACK_STATE_PAUSED) playback.Play();
            }

            this.togglePlayPause();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (playback != null) playback.Next();
        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            if (playback != null) playback.Previous();
        } 

        private void mainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void togglePlayPause()
        {
            Uri resourceUri;
            if (playback.PLAYBACK_STATE == PlaybackControl.PLAYBACK_STATE_PLAYING) resourceUri = new Uri("Resources/pause.png", UriKind.Relative);
            else resourceUri = new Uri("Resources/play.png", UriKind.Relative);

            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;
            playPauseButton.Background = brush;
        }

        public void updateInterface()
        {
            if (playback == null) return;
            titleBox.Content = playback.currentTrack.Artist + " - " + playback.currentTrack.Title;

            foreach (Track track in playback.CurrentPlaylist)
            {
                playlistBox.Items.Add(String.Format("{0}. {1} / {2}", track.TrackNumber.ToString("00"), track.Title));
            }
        }

        public void changeArtwork()  
        {
            if (playback == null || playback.currentTrack.ArtworkImage == null) return;

            Color oColor = Color.FromArgb(255, 33, 33, 33);
            Image oImage = new Image();
            oImage.Source = playback.currentTrack.ArtworkImage;
            oImage.Opacity = 0.4;

            var oGrid = new Grid();  
            var oRectangle = new Rectangle() { Fill = new SolidColorBrush(oColor) };  
            oGrid.Children.Add(oRectangle);  
            oGrid.Children.Add(oImage);  
            var oVisualBrush = new VisualBrush();  
            oVisualBrush.Visual = oGrid;
            oVisualBrush.Stretch = Stretch.UniformToFill;

            mainWindow.Background = oVisualBrush;
        }

        private void playlistButton_Click(object sender, RoutedEventArgs e)
        {
            if (playlistHidden == true)
            {
                mainWindow.Width = 750;
                this.Width = 750;
                playlistHidden = false;
            }
            else
            {
                mainWindow.Width = 525;
                this.Width = 525;
                playlistHidden = true;
            }
        }
    }
}

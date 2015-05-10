using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace pdrPlayer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class window : Window
    {
        PlaybackControl playbackControl;
        ICollectionView view;
        Timer timer;
        Boolean playlistExpanded = false;

        public window()
        {
            InitializeComponent();

            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(updateSeekBar);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void playPauseButton_Click(object sender, RoutedEventArgs e)
        {
            /*if (playbackControl == null)
            {
                VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        playbackControl = new PlaybackControl(dialog.SelectedPath);
                        playbackControl.CurrentPlaylistObject.playlistChanged += new Playlist.playlistChangedHandler(updatePlaylist);
                        playbackControl.TrackChanged += new PlaybackControl.TrackChangedHandler(changeArtwork);
                        playbackControl.TrackChanged += new PlaybackControl.TrackChangedHandler(updateInterface);
                        this.updatePlaylist();
                        this.changeArtwork();
                        this.updateInterface();

                        timer.Start();
                    }
                    catch (Exception exception)
                    {
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
                if (playbackControl.PLAYBACK_STATE == PlaybackControl.PLAYBACK_STATE_PLAYING)
                {
                    timer.Stop();
                    playbackControl.Pause();
                }
                else if (playbackControl.PLAYBACK_STATE == PlaybackControl.PLAYBACK_STATE_PAUSED)
                {
                    timer.Start();
                    playbackControl.Play();
                }
            }

            this.togglePlayPause();*/

            libraryParser lib = new libraryParser();
            var libData = lib.readDat();
            playlistBox.Items.Clear();
            playlistBox.ItemsSource = libData;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            if (playbackControl != null) playbackControl.Next();
            timer.Start();
        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            if (playbackControl != null) playbackControl.Previous();
            timer.Start();
        }

        private void expand_Click(object sender, RoutedEventArgs e)
        {
            if (playlistExpanded == true)
            {
                bottomGrid.Visibility = System.Windows.Visibility.Collapsed;
                playlistExpanded = false;
            }
            else
            {
                bottomGrid.Visibility = System.Windows.Visibility.Visible;
                playlistExpanded = true;
            }
        }

        private void playlistItem_DoubleClick(object sender, EventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            int index = playlistBox.Items.IndexOf(item.Content);
            view.MoveCurrentTo(item.Content);

            timer.Stop();
            playbackControl.Play(index);
            togglePlayPause();
            timer.Start();
        }

        private void settings_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
        }

        //INTERFACE FUNCTIONALITY

        private void togglePlayPause()
        {
            Uri resourceUri;
            if (playbackControl.PLAYBACK_STATE == PlaybackControl.PLAYBACK_STATE_PLAYING) resourceUri = new Uri("Resources/pause.png", UriKind.Relative);
            else resourceUri = new Uri("Resources/play.png", UriKind.Relative);

            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;
            playPauseButton.Background = brush;
        }

        public void updatePlaylist()
        {
            playlistBox.Items.Clear();
            view = CollectionViewSource.GetDefaultView(playbackControl.CurrentPlaylist);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Album"));
            playlistBox.ItemsSource = view;
        }

        public void updateInterface()
        {
            if (playbackControl == null) return;

            progressBar.Value = 0;
            titleBlock.Text = playbackControl.currentTrack.Title;
            artistLabel.Content = playbackControl.currentTrack.Artist + " - " + playbackControl.currentTrack.Album + " (" + playbackControl.currentTrack.Year + ")";
            view.MoveCurrentTo(playbackControl.CurrentPlaylistObject.Get(playbackControl.CURRENT_PLAYLIST_INDEX));
        }

        public void changeArtwork()
        {
            if (playbackControl == null || playbackControl.currentTrack.ArtworkImage == null) return;

            Color oColor = Color.FromArgb(255, 33, 33, 33);
            Image oImage = new Image();
            oImage.Source = playbackControl.currentTrack.ArtworkImage;
            oImage.Opacity = 0.4;

            var oGrid = new Grid();
            var oRectangle = new Rectangle() { Fill = new SolidColorBrush(oColor) };
            oGrid.Children.Add(oRectangle);
            oGrid.Children.Add(oImage);
            var oVisualBrush = new VisualBrush();
            oVisualBrush.Visual = oGrid;
            oVisualBrush.Stretch = Stretch.UniformToFill;

            topGrid.Background = oVisualBrush;
        }

        public void updateSeekBar(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action<ProgressBar>(setValue), progressBar);
        }

        private void setValue(ProgressBar obj)
        {
            double percentagePostion = (double)playbackControl.playbackPosition.Ticks / playbackControl.currentTrack.Duration.Ticks;
            obj.Value = percentagePostion * 100;
        }

        private void progressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (playbackControl == null) return;

            double MousePosition = e.GetPosition(this).X;
            double ratio = MousePosition / progressBar.ActualWidth;

            playbackControl.Seek(ratio);
        }
    }
}
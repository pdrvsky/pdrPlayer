using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using TagLib;

namespace pdrPlayer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class window : Window
    {
        PlaybackControl playbackControl;
        Timer timer;
        Boolean playlistExpanded = true;
        String albumArtworkSet = null;
        KeyListener keyListener = new KeyListener();
        VisualBrush artworkBrush = new VisualBrush();

        List<mediaStruct.Track> sourceList = new List<mediaStruct.Track>();

        internal class ApiCodes
        {
            public const int SC_RESTORE = 0xF120;
            public const int SC_MINIMIZE = 0xF020;
            public const int WM_SYSCOMMAND = 0x0112;
        }

        private IntPtr hWnd;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        public window()
        {
            InitializeComponent();

            playbackControl = new PlaybackControl();
            playbackControl.TrackChanged += new PlaybackControl.TrackChangedHandler(updateInterface);
            playbackControl.TrackChanged += new PlaybackControl.TrackChangedHandler(loadArtwork);

            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(updateSeekBar);

            //HANDLE KEYBOARD SEARCH
            this.KeyDown += new KeyEventHandler(searchArtist);

            //REGISTER VISUAL BRUSH NAME
            this.RegisterName( "artworkBrush", artworkBrush );

            libraryParser lib = new libraryParser();
            var readList = lib.readList();
            if (readList == null) return;
            sourceList = lib.readList().Where(s => s.Artist != null).ToList();
            var arts = lib.readList().GroupBy(g => g.Artist, StringComparer.InvariantCultureIgnoreCase).Select(sel => sel.Key).OrderBy( ord => ord ).ToList();

            LibraryView.ItemsSource = arts;
        }

        public window(string[] args)
        {
            InitializeComponent();

            playbackControl = new PlaybackControl();
            playbackControl.TrackChanged += new PlaybackControl.TrackChangedHandler(updateInterface);
            playbackControl.TrackChanged += new PlaybackControl.TrackChangedHandler(loadArtwork);

            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(updateSeekBar);

            //REGISTER VISUAL BRUSH NAME
            this.RegisterName( "artworkBrush", artworkBrush );

            List<mediaStruct.Track> trackList = new List<mediaStruct.Track>();
            foreach (string path in args)
            {
                if (!path.EndsWith(".mp3")) continue;
                TagLib.File file = TagLib.File.Create( path );
                trackList.Add(new mediaStruct.Track() { Path = path, Artist = file.Tag.FirstArtist ?? "?", Album = file.Tag.Album, Title = file.Tag.Title, Year = file.Tag.Year, TrackNumber = file.Tag.Track, Duration = file.Properties.Duration });
            }

            bottomGrid.Margin = new Thickness(-400,0,0,0);
            ArtistView.ItemsSource = trackList;
        }

        private void Application_Startup(object sender, RoutedEventArgs e)
        {
            hWnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hWnd).AddHook(WindowProc);

            keyListener.OnKeyPressed += globalKeyPressed;
            keyListener.HookKeyboard();
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == ApiCodes.WM_SYSCOMMAND)
            {
                if (wParam.ToInt32() == ApiCodes.SC_MINIMIZE)
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Minimized;
                    handled = true;
                }
                else if (wParam.ToInt32() == ApiCodes.SC_RESTORE)
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.None;
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void globalKeyPressed(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed == Key.MediaPlayPause) this.playPauseButton_Click(sender, new RoutedEventArgs());
        }

        private void Application_Exit(object sender, RoutedEventArgs e)
        {
            keyListener.UnHookKeyboard();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(hWnd, ApiCodes.WM_SYSCOMMAND, new IntPtr(ApiCodes.SC_MINIMIZE), IntPtr.Zero);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void playPauseButton_Click(object sender, EventArgs e)
        {
            if (ArtistView.Items.Count == 0)
            {
                this.LoadArtist(LibraryView.ItemContainerGenerator.ContainerFromIndex(0), e);
                return;
            }
            else if (playbackControl.PLAYBACK_STATE == PlaybackControl.PLAYBACK_STATE_PLAYING)
            {
                timer.Stop();
                playbackControl.Pause();
            }
            else if (playbackControl.PLAYBACK_STATE == PlaybackControl.PLAYBACK_STATE_PAUSED || playbackControl.PLAYBACK_STATE == PlaybackControl.PLAYBACK_STATE_INITIALIZED)
            {
                timer.Start();
                playbackControl.Play();
            }

            this.togglePlayPause();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            timer.Stop();
            if (playbackControl != null) playbackControl.Next();
            timer.Start();
        }

        private void previousButton_Click(object sender, EventArgs e)
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

        private void setTrack ( object sender, EventArgs e )
        {
            ListBoxItem item = sender as ListBoxItem;
            int index = ArtistView.Items.IndexOf(item.Content);
            ArtistView.SelectedIndex = index;

            timer.Stop();
            playbackControl.Play(index);
            togglePlayPause();
            timer.Start();
        }

        private void backClick( object sender, EventArgs e )
        {
            animateBottomGrid(0);
            bottomGrid.DataContext = null;
        }

        private void settings_Click(object sender, EventArgs e)
        {
            foreach (var wnd in Application.Current.Windows)
                if (wnd is Settings) return;

            Settings settings = new Settings();
            settings.Show();
        }

        private void searchArtist(object sender, KeyEventArgs e)
        {
            if (bottomGrid.Margin == new Thickness(0))
            {
                if (e.Key >= Key.A && e.Key <= Key.Z)
                {
                    var search = LibraryView.ItemsSource.Cast<String>().ToList().Where(w => w.StartsWith(e.Key.ToString()));
                    if (search.Count() > 0)
                    {
                        string first = search.First();
                        int item = LibraryView.Items.IndexOf(first);
                        LibraryView.ScrollIntoView(LibraryView.Items.GetItemAt(item));
                    }
                }
            }
            else
            {
                if (e.Key >= Key.A && e.Key <= Key.Z)
                {
                    var albums = ArtistView.Items.Cast<mediaStruct.Track>().Select( s => s.Album ).Distinct().Where( w => w.StartsWith(e.Key.ToString()) );
                    if (albums.Count() >= 1)
                    {
                        var search = ArtistView.Items.Cast<mediaStruct.Track>().Where(w => w.Album.Equals(albums.First()));
                        if (search.Count() >= 1)
                        {
                            int item = ArtistView.Items.IndexOf(search.First());
                            ArtistView.ScrollIntoView(ArtistView.Items.GetItemAt(item));
                        }
                    }
                }
            }
        }

        private void LoadArtist( object sender, EventArgs e )
        {
            ListBoxItem target = sender as ListBoxItem;
            List<mediaStruct.Track> artistTracks = sourceList.Where(w => w.Artist.Equals(target.Content.ToString(), StringComparison.InvariantCultureIgnoreCase))
                .OrderBy( o => o.Year ).ThenBy( o => o.Album ).ThenBy( o => o.TrackNumber ).ToList();

            ArtistGrid.DataContext = artistTracks;
            playbackControl.SetPlaylist( ArtistView.Items.Cast<mediaStruct.Track>().ToList() );

            new System.Threading.Thread(() => {
                Dispatcher.BeginInvoke(new Action(() => { animateBottomGrid(400); }));
            }).Start();

            ArtistView.SelectedIndex = -1;
        }

        //INTERFACE FUNCTIONALITY

        private void animateBottomGrid(uint pos)
        {
            Storyboard story = new Storyboard();
            ThicknessAnimation tAnimation = new ThicknessAnimation();
            tAnimation.From = bottomGrid.Margin;
            tAnimation.To = new Thickness(-pos, 0, 0, 0);
            tAnimation.Duration = TimeSpan.FromSeconds(0.15);
            story.Children.Add(tAnimation);
            Storyboard.SetTargetName(tAnimation, bottomGrid.Name);
            Storyboard.SetTargetProperty(tAnimation, new PropertyPath(MediaElement.MarginProperty));
            Storyboard.SetDesiredFrameRate(tAnimation, 60);
            story.Begin(this);
        }

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

            if (playbackControl.PLAYBACK_STATE == PlaybackControl.PLAYBACK_STATE_PLAYING) resourceUri = new Uri("Resources/thumb_pause.png", UriKind.Relative);
            else resourceUri = new Uri("Resources/thumb_play.png", UriKind.Relative);

            streamInfo = Application.GetResourceStream(resourceUri);
            temp = BitmapFrame.Create(streamInfo.Stream);
            btnPlay.ImageSource = temp;
        }

        public void updateInterface()
        {
            if (playbackControl == null) return;

            progressBar.Value = 0;
            titleBlock.Text = playbackControl.currentTrack.Title;
            artistLabel.Content = playbackControl.currentTrack.Artist + " - " + playbackControl.currentTrack.Album + " (" + playbackControl.currentTrack.Year + ")";
            //view.MoveCurrentTo(playbackControl.CurrentPlaylistObject.Get(playbackControl.CURRENT_PLAYLIST_INDEX));
            ArtistView.SelectedIndex = playbackControl.CURRENT_PLAYLIST_INDEX;
        }

        /*public void changeArtwork()
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
        }*/

        public async void loadArtwork()
        {
            if (playbackControl.currentTrack.Path == null || playbackControl.currentTrack.Album == albumArtworkSet) return;

            TagLib.File trackFile = TagLib.File.Create(playbackControl.currentTrack.Path);
            BitmapImage artwork = null;

            if (trackFile.Tag.Pictures.Length >= 1)
            {
                var bin = (byte[])(trackFile.Tag.Pictures[0].Data.Data);
                MemoryStream ms = new MemoryStream(bin);
                ms.Seek(0, SeekOrigin.Begin);

                // ImageSource for System.Windows.Controls.Image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                artwork = bitmap;
            }
            else
            {
                string path = System.IO.Path.GetDirectoryName(playbackControl.currentTrack.Path);
                List<string> images = Directory.EnumerateFiles(path, "*.*").Where(w => w.EndsWith(".jpg") || w.EndsWith(".png")).ToList();

                if (images.Count > 0) artwork = new BitmapImage(new Uri(images[0]));
            }

            if (artwork != null)
            {
                Kaliko.ImageLibrary.KalikoImage kalImg;
                if (artwork.StreamSource != null) kalImg = new Kaliko.ImageLibrary.KalikoImage(artwork.StreamSource);
                else kalImg = new Kaliko.ImageLibrary.KalikoImage(artwork.UriSource.LocalPath);

                topGrid.Background = new SolidColorBrush(Color.FromArgb(255, 11, 11, 11));
                BitmapImage bmpImg = await blurArtwork(kalImg);

                Color oColor = Color.FromArgb(255, 33, 33, 33);
                Image oImage = new Image();
                oImage.Source = bmpImg;
                oImage.Opacity = 0.4;

                var oGrid = new Grid();
                var oRectangle = new Rectangle() { Fill = new SolidColorBrush(oColor) };
                oGrid.Children.Add(oRectangle);
                oGrid.Children.Add(oImage);
                artworkBrush.Visual = oGrid;
                artworkBrush.Stretch = Stretch.UniformToFill;

                topGrid.Background = artworkBrush;

                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 0;
                opacityAnimation.To = 1;
                opacityAnimation.Duration = TimeSpan.FromSeconds(0.5);
                Storyboard.SetTargetName(opacityAnimation, "artworkBrush");
                Storyboard.SetTargetProperty(
                    opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
                Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
                mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);
                mouseLeftButtonDownStoryboard.Begin(this);

                albumArtworkSet = playbackControl.currentTrack.Album;
            }
            else
            {
                topGrid.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                albumArtworkSet = playbackControl.currentTrack.Album;
            }
        }

        private async Task<BitmapImage> blurArtwork( Kaliko.ImageLibrary.KalikoImage img )
        {
            return await Task<BitmapImage>.Run(() =>
            {
                Kaliko.ImageLibrary.Filters.GaussianBlurFilter gaussianBlur = new Kaliko.ImageLibrary.Filters.GaussianBlurFilter(6);
                img.ApplyFilter(gaussianBlur);

                BitmapImage resImg = new BitmapImage();
                MemoryStream ms = new MemoryStream();
                img.SaveBmp(ms);
                ms.Seek(0, SeekOrigin.Begin);
                resImg.BeginInit();
                resImg.StreamSource = ms;
                resImg.EndInit();
                resImg.Freeze();

                return resImg;
            });
        }

        public void updateSeekBar(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action<ProgressBar>(setValue), progressBar);
        }

        private void setValue(ProgressBar obj)
        {
            double percentagePostion = (double)playbackControl.playbackPosition.Ticks / playbackControl.currentTrack.Duration.Ticks;
            if (!double.IsInfinity(percentagePostion)) obj.Value = percentagePostion * 100;
            else obj.Value = 0;
        }

        private void progressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (playbackControl == null) return;

            double MousePosition = e.GetPosition(this).X;
            double ratio = MousePosition / progressBar.ActualWidth;

            this.updateSeekBar(sender, e);
            playbackControl.Seek(ratio);
        }

        private void plrWindow_Activated(object sender, EventArgs e)
        {
            //sbase.OnActivated(e);

            if (WindowStyle != WindowStyle.None)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (DispatcherOperationCallback)delegate(object unused)
                {
                    WindowStyle = WindowStyle.None;
                    return null;
                }
               , null);
            }
        }
    }   
}
using System;
using System.IO;
using System.Collections.Generic;
using NAudio;
using NAudio.Wave;
using System.Timers;

namespace pdrPlayer
{
    class PlaybackControl
    {
        //TRACK CHANGE EVENT
        public delegate void TrackChangedHandler();
        public event TrackChangedHandler TrackChanged; 

        //CLASSES
        IWavePlayer waveOutDevice;
        AudioFileReader audioFileReader;
        Playlist playlist;

        //PLAYBACK STATES
        public const int PLAYBACK_STATE_PLAYING = 1;
        public const int PLAYBACK_STATE_PAUSED = 2;
        public const int PLAYBACK_STATE_STOPPED = 3;

        //CLASS FIELDS
        public int PLAYBACK_STATE;
        public int CURRENT_PLAYLIST_INDEX;
        public Track currentTrack;

        string path;
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (!Directory.Exists(value))
                    throw new DirectoryNotFoundException();
                else
                    this.path = value;
            }
        }

        public TimeSpan playbackPosition { get { if (audioFileReader != null) return audioFileReader.CurrentTime; else return new TimeSpan(); } }
        public List<Track> CurrentPlaylist { get { return this.playlist.trackList; } }
        public Playlist CurrentPlaylistObject { get { return this.playlist; } }

        public PlaybackControl(string path)
        {
            try
            {
                this.Path = path;
            }
            catch (DirectoryNotFoundException e)
            {
                throw new Exception("Initialization error.");
            }

            waveOutDevice = new WaveOut();
            waveOutDevice.PlaybackStopped += new EventHandler<StoppedEventArgs>(playbackStopped);
            playlist = new Playlist(this.Path);

            this.playTrack();
        }

        public void Play() {
            if (playlist == null)
                throw new Exception("Tried to play null playlist.");

            if (PLAYBACK_STATE == PLAYBACK_STATE_PAUSED)
            {
                waveOutDevice.Play();
                PLAYBACK_STATE = PLAYBACK_STATE_PLAYING;
            }
            else if (PLAYBACK_STATE == PLAYBACK_STATE_PLAYING)
            {
                waveOutDevice.Pause();
                PLAYBACK_STATE = PLAYBACK_STATE_PAUSED;
            }
        }

        public void Play(int index)
        {
            if (playlist == null)
                throw new Exception("Tried to play null playlist.");

            if (index >= 0 && index <= playlist.Length)
            {
                CURRENT_PLAYLIST_INDEX = index;
                this.playTrack();
            }
        }

        public void Pause() {
            if( waveOutDevice == null )
                throw new Exception();

            waveOutDevice.Pause();
            PLAYBACK_STATE = PLAYBACK_STATE_PAUSED;
        }

        public void Next() {
            if (CURRENT_PLAYLIST_INDEX + 2 > playlist.Length) return;

            CURRENT_PLAYLIST_INDEX++;
            this.playTrack();
        }

        public void Previous() {
            if (CURRENT_PLAYLIST_INDEX - 1 < 0) return;

            CURRENT_PLAYLIST_INDEX--;
            this.playTrack();
        }

        private void playbackStopped(object sender, EventArgs e)
        {
            this.Next();
        }

        private void playTrack()
        {
            try
            {
                this.disposeWave();
                currentTrack = playlist.Get(CURRENT_PLAYLIST_INDEX);
                audioFileReader = new AudioFileReader(currentTrack.Path);
                waveOutDevice.Init(audioFileReader);
                waveOutDevice.Play();

                PLAYBACK_STATE = PLAYBACK_STATE_PLAYING;

                if (TrackChanged != null)
                    TrackChanged();
            }
            catch (Exception e)
            {
                Console.WriteLine( e.Message );
            }
        }

        private void disposeWave()
        {
            if (audioFileReader != null)
            {
                audioFileReader.Dispose();
            }

            if (currentTrack != null)
            {
                currentTrack = null;
            }
        }

        public void Seek( double ratio )
        {
            TimeSpan newPos = new TimeSpan((long)(currentTrack.Duration.Ticks * ratio));
            audioFileReader.Seek((long)(newPos.TotalSeconds * audioFileReader.WaveFormat.AverageBytesPerSecond), SeekOrigin.Begin);
        }
    }
}

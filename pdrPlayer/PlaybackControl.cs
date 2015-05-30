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

        //LIBRARY LIST
        List<mediaStruct.Track> libraryList;

        //PLAYBACK STATES
        public const int PLAYBACK_STATE_PLAYING = 1;
        public const int PLAYBACK_STATE_PAUSED = 2;
        public const int PLAYBACK_STATE_INITIALIZED = 3;

        //CLASS FIELDS
        public int PLAYBACK_STATE;
        public int CURRENT_PLAYLIST_INDEX;
        public mediaStruct.Track currentTrack;

        public TimeSpan playbackPosition { 
            get { if (audioFileReader != null) return audioFileReader.CurrentTime; else return new TimeSpan(); } 
        }

        public PlaybackControl()
        {
            waveOutDevice = new WaveOut();
            waveOutDevice.PlaybackStopped += new EventHandler<StoppedEventArgs>(playbackStopped);

            PLAYBACK_STATE = PLAYBACK_STATE_INITIALIZED;
        }

        public void SetPlaylist( List<mediaStruct.Track> playlist )
        {
            libraryList = playlist;
            CURRENT_PLAYLIST_INDEX = 0;

            if (PLAYBACK_STATE != PLAYBACK_STATE_INITIALIZED)
                PLAYBACK_STATE = PLAYBACK_STATE_INITIALIZED;
        }

        public void Play() {
            if (libraryList == null)
                throw new Exception("Tried to play null playlist.");

            switch (PLAYBACK_STATE)
            {
                case PLAYBACK_STATE_INITIALIZED:
                    this.playTrack();
                    break;
                case PLAYBACK_STATE_PLAYING:
                    waveOutDevice.Pause();
                    PLAYBACK_STATE = PLAYBACK_STATE_PAUSED;
                    break;
                case PLAYBACK_STATE_PAUSED:
                    waveOutDevice.Play();
                    PLAYBACK_STATE = PLAYBACK_STATE_PLAYING;
                    break;
            }
        }

        public void Play(int index)
        {
            if (libraryList == null)
                throw new Exception("Tried to play null playlist.");

            if (index >= 0 && index <= libraryList.Count)
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
            if (CURRENT_PLAYLIST_INDEX + 2 > libraryList.Count) return;

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
                this.DisposeWave();
                currentTrack = libraryList[CURRENT_PLAYLIST_INDEX];
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

        public void DisposeWave()
        {
            if (audioFileReader != null)
            {
                waveOutDevice.Stop();
                audioFileReader.Dispose();
            }
        }

        public void Seek( double ratio )
        {
            TimeSpan newPos = new TimeSpan((long)(currentTrack.Duration.Ticks * ratio));
            audioFileReader.Seek((long)(newPos.TotalSeconds * audioFileReader.WaveFormat.AverageBytesPerSecond), SeekOrigin.Begin);
        }
    }
}

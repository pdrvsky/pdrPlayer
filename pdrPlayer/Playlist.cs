using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace pdrPlayer
{
    class Playlist
    {
        //PLAYLIST CHANGED EVENT
        public delegate void playlistChangedHandler();
        public event playlistChangedHandler playlistChanged;

        string path;
        string[] fileList;
        public List<Track> trackList;

        uint length;
        public uint Length
        {
            get { return this.length; }
        }

        public Playlist(string path)
        {
            this.path = path;
            if( this.path == null )
                throw new Exception("Playlist init error.");

            fileList = Directory.EnumerateFiles( this.path, "*.*", SearchOption.AllDirectories )
                .Where( file => file.ToLower().EndsWith(".mp3") || file.ToLower().EndsWith(".flac") || file.ToLower().EndsWith(".mp4") ).ToArray();

            trackList = new List<Track>();
            foreach (string filePath in fileList)
            {
                trackList.Add( new Track( filePath ) );
            }

            trackList = this.Sort( trackList );
            this.length = (uint)trackList.Count;

            if (playlistChanged != null) playlistChanged();
        }

        public Track Get(int index)
        {
            return trackList[index];
        }

        private List<Track> Sort(List<Track> unsortedList)
        {
            return trackList.OrderBy(o1 => o1.Year).ThenBy(o2 => o2.Artist).ThenBy(o3 => o3.Album).ThenBy(o4 => o4.TrackNumber).ToList();
        }
    }
}

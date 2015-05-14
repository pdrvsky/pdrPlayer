using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace pdrPlayer.mediaStruct
{
    [Serializable]
    public struct Track
    {
        public string Path { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public uint Year { get; set; }
        public uint TrackNumber { get; set; }
        public TimeSpan Duration { get; set; }
        [NonSerialized]
        public BitmapImage Artwork;
    }

    [Serializable]
    public struct Artist
    {
        public string ArtistName { get; set; }
        public Dictionary<string, List<Track>> Albums { get; set; }
    }
}

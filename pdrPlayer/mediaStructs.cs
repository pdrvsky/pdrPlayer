using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace pdrPlayer.mediaStruct
{
    [Serializable]
    public struct Track
    {
        public string Artist;
        public string Album;
        public string Title;
        public uint Year;
        public uint TrackNumber;
        public TimeSpan Duration;
        [NonSerialized]
        public BitmapImage Artwork;
    }

    [Serializable]
    public struct Artist
    {
        public string ArtistName;
        public Dictionary<string, List<Track>> Albums;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using TagLib;

namespace pdrPlayer
{
    class Track
    {
        uint year;
        uint trackNumber;
        TimeSpan duration;
        string title;
        string album;
        string artist;
        string path;

        BitmapImage artworkImage;

        public uint Year { get { return this.year; } set { this.year = value; } }
        public uint TrackNumber { get { return this.trackNumber; } set { this.trackNumber = value; } }
        public TimeSpan Duration { get { return this.duration; } }
        public string Title { get { return this.title; } set { this.title = value; } }
        public string Album { get { return this.album; } set { this.album = value; } }
        public string Artist { get { return this.artist; } set { this.artist = value; } }
        public string Path { get { return this.path; } set { this.path = value; } }
        public BitmapImage ArtworkImage { get { return this.artworkImage; } }

        public Track( string filePath )
        {
            TagLib.File file = TagLib.File.Create(filePath);
            this.year = file.Tag.Year;
            this.trackNumber = file.Tag.Track;
            this.duration = file.Properties.Duration;
            this.title = file.Tag.Title;
            this.album = file.Tag.Album;
            this.artist = file.Tag.Artists[0];
            this.path = filePath;

            if (file.Tag.Pictures.Length >= 1)
            {
                var bin = (byte[])(file.Tag.Pictures[0].Data.Data);
                MemoryStream ms = new MemoryStream(bin);
                ms.Seek(0, SeekOrigin.Begin);

                // ImageSource for System.Windows.Controls.Image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                this.artworkImage = bitmap;
            }
            else
            {
                this.artworkImage = searchForArtwork();
            }

            file.Dispose();
        }

        private BitmapImage searchForArtwork()
        {
            string path = System.IO.Path.GetDirectoryName(this.path);
            List<string> images = Directory.EnumerateFiles(path, "*.*").Where( w => w.EndsWith("folder.jpg") || w.EndsWith("folder.png") ).ToList();

            if (images.Count > 0) return new BitmapImage(new Uri(images[0]));
            else return null;
        }
    }
}

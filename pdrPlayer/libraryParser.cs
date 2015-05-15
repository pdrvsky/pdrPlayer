using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using System.Runtime.Serialization.Formatters.Binary;

namespace pdrPlayer
{
    class libraryParser
    {
        public Dictionary<string, mediaStruct.Artist> artistDict = new Dictionary<string, mediaStruct.Artist>();
        
        public delegate void parsingFinishedHandler();
        public event parsingFinishedHandler parsingFinished;
        public delegate void filesParsedHand( int files, int count );
        public event filesParsedHand filesParsed;

        TagLib.File tagFile;

        public libraryParser() { }

        public libraryParser(string path)
        {
            Task parseTask = new Task( () => this.parse(path) );
            parseTask.Start();
        }

        #region MainTask

        private void parse(string path) {
            List<string> files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(e => e.EndsWith(".mp3")).ToList();
            List<mediaStruct.Track> tracks = new List<mediaStruct.Track>();
            int fileCount = 0;

            foreach (string file in files)
            {
                fileCount++;
                try
                {
                    tagFile = TagLib.File.Create(file);
                    tracks.Add(new mediaStruct.Track() { Path = file, Artist = tagFile.Tag.FirstArtist ?? "?", Album = tagFile.Tag.Album, Title = tagFile.Tag.Title, Year = tagFile.Tag.Year, TrackNumber = tagFile.Tag.Track, Duration = tagFile.Properties.Duration });
                }
                catch (Exception) { }

                if (filesParsed != null) filesParsed(fileCount, files.Count);
            }

            /*Dictionary<string, List<mediaStruct.Track>> artists = tracks.OrderBy(o2 => o2.Artist).ThenBy(o3 => o3.Album)
                                                    .GroupBy(gr => gr.Artist).ToDictionary(td => td.Key, td2 => td2.ToList());

            foreach (KeyValuePair<string, List<mediaStruct.Track>> artist in artists)
            {
                Dictionary<string, List<mediaStruct.Track>> albums = artist.Value.GroupBy(gr => gr.Album).ToDictionary(x => x.Key, x => x.ToList());
                artistDict.Add(artist.Key, new mediaStruct.Artist { ArtistName = artist.Key, Albums = albums });
            }*/

            this.saveList(tracks);
        }

        private void saveDat(Dictionary<string, mediaStruct.Artist> dictionary)
        {
            try
            {
                BinaryFormatter binFor = new BinaryFormatter();
                FileStream fs = new FileStream(Environment.CurrentDirectory + "\\db.dat", FileMode.Create, FileAccess.Write);
                binFor.Serialize(fs, dictionary.Values.ToArray());
                fs.Close();

                if (parsingFinished != null) parsingFinished();
            }
            catch (Exception) { }
        }

        private void saveList(List<mediaStruct.Track> list)
        {
            try
            {
                BinaryFormatter binFor = new BinaryFormatter();
                FileStream fs = new FileStream(Environment.CurrentDirectory + "\\db.dat", FileMode.Create, FileAccess.Write);
                binFor.Serialize(fs, list.ToArray());
                fs.Close();

                if (parsingFinished != null) parsingFinished();
            }
            catch (Exception) { }
        }

        public Dictionary<string, mediaStruct.Artist> readDat()
        {
            try
            {
                BinaryFormatter binFor = new BinaryFormatter();
                FileStream fs = new FileStream(Environment.CurrentDirectory + "\\db.dat", FileMode.Open, FileAccess.Read);
                mediaStruct.Artist[] a_artists = (mediaStruct.Artist[])binFor.Deserialize(fs);

                return a_artists.ToDictionary(t => t.ArtistName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<mediaStruct.Track> readList()
        {
            try
            {
                BinaryFormatter binFor = new BinaryFormatter();
                FileStream fs = new FileStream(Environment.CurrentDirectory + "\\db.dat", FileMode.Open, FileAccess.Read);
                mediaStruct.Track[] a_artists = (mediaStruct.Track[])binFor.Deserialize(fs);

                return a_artists.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}

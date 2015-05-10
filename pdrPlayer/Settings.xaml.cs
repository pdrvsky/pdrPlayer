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
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;
using System.Xml.Linq;

namespace pdrPlayer
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        libraryParser lib;
        Dictionary<string, string> set;

        public Settings()
        {
            InitializeComponent();
            set = readSettings();
            updateSettings(set);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                set["media_root"] = dialog.SelectedPath;
                updateSettings(set);

                lib = new libraryParser(dialog.SelectedPath);
                lib.parsingFinished += new libraryParser.parsingFinishedHandler(msgBox);
                lib.filesParsed += new libraryParser.filesParsedHand(update);
                WindowTitle.Content += " - Parsowanie...";
            }
            else
            {
                return;
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (set != null) saveSettings(set);
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveSettings(Dictionary<string, string> settings)
        {
            XElement xml = new XElement("settings", settings.Select( sel => new XElement( sel.Key, sel.Value ) ));
            xml.Save("settings.xml");
        }

        private Dictionary<string, string> readSettings()
        {
            XElement settings = XElement.Load("settings.xml");
            return settings.Elements().ToDictionary( key => key.Name.ToString(), val => val.Value );
        }

        private void msgBox()
        {
            WindowTitle.Dispatcher.Invoke(() => { WindowTitle.Content = "Ustawienia"; });
            MessageBox.Show("Parsing Finished!");
        }

        private void update( int f, int c )
        {
            WindowTitle.Dispatcher.Invoke(() => { WindowTitle.Content = String.Format("Ustawienia - Parsowanie... {0:00}%", (double)f/c*100); });
        }

        private void updateSettings( Dictionary<string, string> settings )
        {
            mediaRoot.Text = settings["media_root"];
        }

        private void pdrSettings_Unloaded(object sender, RoutedEventArgs e)
        {
            if (lib != null) lib.filesParsed -= update;
        }
    }
}

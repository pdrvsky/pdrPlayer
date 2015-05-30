
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace pdrPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void AppStartup(object sender, StartupEventArgs e)
        {
            window wnd;

            if (e.Args.Length > 0) wnd = new window(e.Args);
            else wnd = new window();

            wnd.Show();
        }
    }
}

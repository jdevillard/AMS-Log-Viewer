using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AzureMobileServiceLogViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AzureMobileServiceLogViewer");
            // Set |DataDirectory| value
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
            base.OnStartup(e);
        }
    }
}

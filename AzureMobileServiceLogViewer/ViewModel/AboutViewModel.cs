using AzureMobileServiceLogViewer.Command;
using AzureMobileServiceLogViewer.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AzureMobileServiceLogViewer.ViewModel
{
    public class AboutViewModel : INotifyPropertyChanged
    {
        #region properties
        private View.About _aboutView;
        private Boolean _isNewVersionAvailable;
        private String _currentVersion;
        private String _availableVersion;
        private OnlineVersion _onlineVersion;

        public Boolean IsNewVersionAvailable {
            get { return _isNewVersionAvailable; }
            private set { _isNewVersionAvailable = value; OnPropertyChanged("IsNewVersionAvailable"); }
        }

        public String CurrentVersion
        {
            get { return _currentVersion; }
            private set { _currentVersion = value; OnPropertyChanged("CurrentVersion"); }
        }

        public String AvailableVersion { 
            get { return _availableVersion; }
            private set { _availableVersion = value; OnPropertyChanged("AvailableVersion"); }
        }
        public String TextAbout
        {
            get
            {
                return "Azure Mobile Log Viewer" + System.Environment.NewLine +
                   "Jérémie Devillard " + System.Environment.NewLine +
                   "http://jeremiedevillard.wordpress.com";
            }
        }
        #endregion

        
        #region Command
        public ICommand btnLater
        {
            get {
                return new DelegateCommand((o)=>  
                {
                    _aboutView.Close();
                });
            }
        }

        public ICommand btnUpdate
        {
            get
            {
                return new DelegateCommand((o) =>
                {
                    if(IsNewVersionAvailable)
                    {
                        //start process url
                        System.Diagnostics.Process.Start(_onlineVersion.downloadurl);
                        _aboutView.Close();
                    }
                    else
                    {
                        _aboutView.Close();
                    }
                });
            }
        }
        #endregion
        public AboutViewModel(View.About aboutView)
        {
            
            _aboutView = aboutView;

            _aboutView.NavigateToSiteLink.RequestNavigate += NavigateToSiteLink_RequestNavigate;

            IsNewVersionAvailable = false;
            CheckForUpdate(GetCurrentVersion());
        }

        void NavigateToSiteLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #region UpdateChecker
        private String GetCurrentVersion()
        {
            Assembly oAssembly = Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(oAssembly.Location);

            _currentVersion = versionInfo.FileMajorPart.ToString() + "." +
                                     versionInfo.FileMinorPart.ToString() + "." +
                                     versionInfo.FileBuildPart.ToString() + "." +
                                     versionInfo.FilePrivatePart.ToString();

            string currentVersion = _currentVersion.Replace(".", String.Empty);
            return currentVersion;
        }

        private void CheckForUpdate(string currentVersion)
        {
            try
            {
                var latestUpdate = UpdateChecker.CheckForUpdates("http://jdevillard.blob.core.windows.net/codeplex/amslogviewer/release.xml", UpdateFilter.Beta);
                if (latestUpdate != null)
                {
                    _onlineVersion = latestUpdate;
                    var latestVersion = latestUpdate.newversion.Replace(".", String.Empty);
                    if (Int32.Parse(latestVersion) > Int32.Parse(currentVersion))
                    {
                        IsNewVersionAvailable = true;
                        AvailableVersion = latestUpdate.newversion;
                        _aboutView.btnUpdate.Content = "Update";
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}

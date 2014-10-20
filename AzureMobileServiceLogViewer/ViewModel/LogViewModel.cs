using System.Diagnostics;
using AzureMobileServiceLogViewer.Command;
using AzureMobileServiceLogViewer.Data;
using AzureMobileServiceLogViewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace AzureMobileServiceLogViewer.ViewModel
{
    public class LogsViewModel : INotifyPropertyChanged
    {
        #region Properties
        private MainWindow _mainWindow;

        private ObservableCollection<Subscription> _subscriptions;
        private ObservableCollection<String> _mobileServiceList;
        
        private ICollectionView _logView;
        private ICommand _loadService;
        private String _serviceName;
        private Boolean _loadingString;
        private PublishDataPublishProfile _publishProfile;
        
        private int _nbRows;
        private string _message;
        private bool _autoRefresh;

        private bool _loadingRingMessage;
        private bool _loadingInProgress;

        private System.Timers.Timer _timer;
        private const double _timerInterval = 60 * 1000;

        public bool LoadingInProgress
        {
            get { return _loadingInProgress; }
            set { _loadingInProgress = value; OnPropertyChanged("LoadingInProgress"); }
        }
        public bool LoadingRingMessage
        {
            get { return _loadingRingMessage; }
            private set { _loadingRingMessage = value;
            OnPropertyChanged("LoadingRingMessage");
            }
        }


        public bool AutoRefresh
        {
            get { return _autoRefresh; }
            private set { 
                _autoRefresh = value;
                if (_autoRefresh)
                    StartTimer();
                else
                    _timer.Stop();

                OnPropertyChanged("AutoRefresh");
            }
        }
      
        public String Message
        {
            get { return _message; }
            private set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }
        public int NbRows
        {
            get
            {
                return _nbRows;
            }
            private set
            {
                _nbRows = value;
                OnPropertyChanged("NbRows");
            }
        }
        public ObservableCollection<Subscription> Subscriptions
        {
            get { return _subscriptions; }
            private set { _subscriptions = value; OnPropertyChanged("Subscriptions"); }
        }

        public PublishDataPublishProfile PublishProfile
        {
            get { return _publishProfile; }
            set { _publishProfile = value; OnPropertyChanged("PublishDataPublishProfile"); }
        }

        public ObservableCollection<String> MobileServiceList
        {
            get { return _mobileServiceList; }
            set
            {
                _mobileServiceList = value; 
               // OnPropertyChanged("MobileServiceList");
            }
        }

        public Boolean LoadingRing
        {
            get
            {
                return _loadingString;
            }
            set
            {
                _loadingString = value;
                OnPropertyChanged("LoadingRing");
            }
        }
        public ICommand LoadServiceCommand
        {
            get
            {
                return _loadService;
            }
        }
        public ICollectionView LogView
        {
            get { return _logView; }
        }

        public String ServiceName
        {
            get { return _serviceName; }
            set
            {
                _serviceName = value;
                OnPropertyChanged("ServiceName");
            }
        }

        public ICommand OpenSettingWindows
        {
            get
            {
                return new DelegateCommand((o) =>
                {
                    ImportSettingsOpen();
                });
            }
        }

        ObservableCollection<Result> logService
        {
            get;
            set;
        }

        private DateTime _nextRefreshDate;

        public DateTime NextRefreshDate
        {
            get
            {
                return _nextRefreshDate;
            }
            private set {
                _nextRefreshDate = value;
                OnPropertyChanged("NextRefreshDate");
            }
        }

        #endregion

        #region Constructor
        public LogsViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            //Events
            _mainWindow.btnSearch.Click += SearchEvent;
            _mainWindow.btnReset.Click += ResetEvent;
            _mainWindow.cbxSubscription.SelectionChanged += SubscriptionChanged;
            _mainWindow.LogTypeComboBox.SelectionChanged += ComboBox_SelectionChanged;
            _mainWindow.DatePicker.SelectedDateChanged += DatePicker_SelectedDateChanged;
            _mainWindow.cbxMobileServiceName.SelectionChanged += MobileServiceName_SelectionChanged;
            
            //Init
            Subscriptions = new ObservableCollection<Subscription>();
            MobileServiceList = new ObservableCollection<string>();
            

            //Get Subscriptions 
            var dbcon = new DbConnection();
            foreach (var item in dbcon.GetSubscription())
            {
                //Application.Current.Dispatcher.Invoke();
                Subscriptions.Add(item);
            }


            //Get Mobile Service

            _loadService = new DelegateCommandBW((o) =>
            {
                LoadServiceAction(true);
            });
           
            logService = new ObservableCollection<Result>();
            _logView = CollectionViewSource.GetDefaultView(logService);

            _timer = new System.Timers.Timer(_timerInterval);
            _timer.Elapsed += (sender, args) =>
            {
                _timer.Stop();
                LoadServiceAction();
                StartTimer();
                
            };

            AutoRefresh = true;
        }
        #endregion

        #region auto refresh Timer
        private void StartTimer()
        {
            var selectedServiceName = String.Empty;

            Application.Current.Dispatcher.Invoke(()=>
                {
                    selectedServiceName = (string)_mainWindow.cbxMobileServiceName.SelectedValue;
                });

            if (!String.IsNullOrEmpty(selectedServiceName))
            {
                _timer.Start();
                NextRefreshDate = DateTime.Now.AddMilliseconds(_timerInterval);
            }
        }

        private void StopTimer()
        {
            _timer.Stop();
        }
        #endregion


        #region Events
        private void MobileServiceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var mobileServiceSelected = e.AddedItems[0] as String;
                
                var command = new DelegateCommandBW((o) =>
                {
                    this.LoadService(o as string);
                    StartTimer();
                });
                command.Execute(mobileServiceSelected);
            }

            
        }
        
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterList();
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterList();
        }
        private void SubscriptionChanged(object sender, SelectionChangedEventArgs e)
        {
            StopTimer();
            if (e.AddedItems.Count > 0)
            {
                var subscriptionSelected = e.AddedItems[0] as Models.Subscription;
                new DelegateCommandBW((o)=>this.SubscriptionsChanged(o as Models.Subscription))
                    .Execute(subscriptionSelected);
            }
        }
        private void SearchEvent(object sender, RoutedEventArgs e)
        {

            //logview.FilterMessage(tbMessage.Text, DatePicker.SelectedDate);
            FilterList();
        }

        private void ResetEvent(object sender, RoutedEventArgs e)
        {
            this.ResetFilter();
            _mainWindow.tbMessage.Text = null;
            _mainWindow.DatePicker.SelectedDate = null;
            _mainWindow.LogTypeComboBox.SelectedIndex = 0;
        }
        #endregion

        private IList<Result> InsertDbData(string ServiceName, IList<Result> result)
        {
            var conn = new DbConnection(ServiceName);
            conn.InsertData(result);
            //var r = conn.GetData();
            //if (r.Count() == 0)
            //    {
            //        conn.InsertData(result);
            //        return result;
            //    }
            //    else
            //    {
            //        return r.ToList();
            //    }
            return result;
        }

        private void LoadServiceAction()
        {
            LoadServiceAction(false);
        }

        private void LoadServiceAction(bool forceRefresh)
        {
            //Get Selected Mobile Service Name
            Application.Current.Dispatcher.Invoke(() =>
            {
                ServiceName = _mainWindow.cbxMobileServiceName.SelectedValue as String;
            });

            LoadService(ServiceName,forceRefresh);
        }

        public void LoadService(String serviceName){
            LoadService(serviceName, false);
        }
        public void LoadService(String serviceName, bool Force)
        {
            Application app = System.Windows.Application.Current;
            LoadingRing = true;

            app.Dispatcher
                .Invoke(()=>_logView.SortDescriptions.Add(new SortDescription("timeCreated", ListSortDirection.Descending)));


            
            //Create connection
            //Get Data From DataBase If Exist
            if (String.IsNullOrEmpty(serviceName))
            {
                MessageBox.Show("No Service Selected");
                LoadingRing = false;
                return;
            }
            var conn = new DbConnection(serviceName);
            var resultFromDb = conn.GetData() as List<Result>;

            if (app != null)
                app.Dispatcher.Invoke(() => logService.Clear());
            
            resultFromDb.ForEach(u =>
            {
                if (app != null)
                    app.Dispatcher.Invoke(new Action<Result>((o) => { logService.Add(o); }), u);
                
            });

            var logToInsert = new List<Result>();
            Result lastResult = null;

            if (resultFromDb.Count() > 0)
            {
                lastResult = resultFromDb.OrderByDescending(u => u.timeCreated).First();
            }
            
            if(AutoRefresh || Force)
            {
                var results = Data.Helper.GetLog(serviceName);
                var continuationToken = string.Empty;
                while (results != null)
                { 
                    logToInsert = new List<Result>();
                    continuationToken = results.continuationToken;
                    foreach(var item in  results.results)
                    {
                       
                        if (lastResult != null)
                        {
                            if (lastResult.timeCreated < item.timeCreated)
                            {
                                logToInsert.Add(item);
                               // resultFromDb.Add(item);
                                if (app != null)
                                    app.Dispatcher.Invoke(new Action<Result>((o) => { logService.Add(o); }), item);
                            }
                            else //no more request needed
                            {
                                continuationToken = string.Empty;
                                break;
                            }
                        }
                        else
                        {
                            logToInsert.Add(item);

                            if (app != null)
                                app.Dispatcher.Invoke(new Action<Result>((o) => { logService.Add(o); }), item);
                        }

                    };
                    IList<Result> logResult = InsertDbData(serviceName, logToInsert);
                    
                    OnPropertyChanged("logService");
                    if (String.IsNullOrEmpty(continuationToken))
                        break;
                    results = Data.Helper.GetPrivateLog(serviceName, continuationToken);
                }

                
            }
            
            LoadingRing = false;
        }


        #region Filtering DataGrid View
        private void FilterList()
        {
            var type = (_mainWindow.LogTypeComboBox.SelectedValue as ComboBoxItem).Content as String;
            var message = String.Empty;
            if (_mainWindow.tbMessage != null)
                message = _mainWindow.tbMessage.Text;
            DateTime? date = null;
            if (_mainWindow.DatePicker != null)
                date = _mainWindow.DatePicker.SelectedDate;
            this.FilterMessage(message, date, type);
        }
        public void FilterMessage(string message, DateTime? datePicker, String logType)
        {
            _logView.Filter = delegate(object item)
            {
                Result result = item as Result;
                
                var dateFilter = true;
                if (datePicker.HasValue)
                    dateFilter = result.timeCreated.ToShortDateString() == datePicker.Value.ToShortDateString();

                var typeFilter = true;
                if (logType != "All")
                    typeFilter = result.type.ToLower() == logType.ToLower() ;

                return dateFilter && result.message.Contains(message) && typeFilter;

            };
            _logView.Refresh();

        }

        public void ResetFilter()
        {
            _logView.Filter = null;
            _logView.Refresh();
        }
        #endregion


        #region publish profile
        public void LoadPublishProfile(PublishDataPublishProfile profile_)
        {
            new DelegateCommandBW((o) =>
            {
                try
                {
                    
                var profile = o as PublishDataPublishProfile;
                LoadingRingMessage = true;
                Message = "Loading Publish Profile";
                    if (profile.Subscription.Count() != 0)
                    {
                        var _sub = new List<Subscription>();

                        MobileDbContext ctx = new MobileDbContext();

                        foreach (var subscription in profile.Subscription)
                        {
                            _sub.Add(new Subscription()
                            {
                                Id = Guid.Parse(subscription.Id),
                                Name = subscription.Name,
                                Cert = subscription.ManagementCertificate
                            });

                            var subId = Guid.Parse(subscription.Id);
                            //Check If subscription already exist in the database
                            if (!ctx.Subscriptions.Any(u => u.Id == subId))
                            {
                                Message = String.Format("New Subscription : {0} added", subscription.Name);
                                var subToInsert = new Subscription()
                                {
                                    Id = subId,
                                    Name = subscription.Name,
                                    Cert = subscription.ManagementCertificate
                                };
                                ctx.Subscriptions.Add(subToInsert);
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    Subscriptions.Add(subToInsert);
                                });
                            }
                                //Else Check if the certificate has changed, take the last certificate
                            else if (ctx.Subscriptions.Find(subId) != null)
                            {
                                var sub = ctx.Subscriptions.Find(subId);
                                if (sub.Cert != subscription.ManagementCertificate)
                                {
                                    sub.Cert = subscription.ManagementCertificate;
                                    Message = String.Format("Certificate change for Subscription :{0}",
                                        subscription.Name);
                                }

                            }
                        }
                        ctx.SaveChanges();
                        Message = "Publish Profile Loaded";
                    }
                    else
                    {
                        Message = "No subscription found in the publish profile";
                    }
                
                }
                catch (Exception e)
                {
                    Message = e.Message;
                    
                }

                finally
                {
                     LoadingRingMessage = false;
                }
            }).Execute(profile_);
            
        }
        
        private void ImportSettingsOpen()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Publish Profile | *.publishsettings";
            dialog.DefaultExt = ".publishsettings";

            var result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;

                using (var stream = System.IO.File.OpenRead(filename))
                {
                    XmlSerializer sr = new XmlSerializer(typeof (Models.PublishData));
                    try
                    {
                        var o = (Models.PublishData) sr.Deserialize(stream);
                        var publishProfile = o.Items[0];

                        LoadPublishProfile(publishProfile);
                    }
                    catch (Exception)
                    {
                        var resultMessageBox = MessageBox.Show("Error, unable to parse the file, you should use a Subscription Publish Settings downloadable at \n https://windows.azure.com/download/publishprofile.aspx",
                            "Download Publish Profile?",MessageBoxButton.YesNo);
                        if (resultMessageBox == MessageBoxResult.Yes)
                        {
                            Process.Start(new ProcessStartInfo("https://windows.azure.com/download/publishprofile.aspx"));
                        }
                        else
                        {
                            
                        }

                    }
                }
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

        #region Get Mobile Service List
        internal void SubscriptionsChanged(Subscription subscription)
        {
            LoadingRingMessage = true;                
            Message = String.Format("Get Mobile Service From Subscription : {0} ", subscription.Name);
            Application.Current.Dispatcher.Invoke(() => MobileServiceList.Clear());
            
            //Update list of mobile service
            MobileDbContext ctx = new MobileDbContext();
            var mobileService = ctx.MobileServices.Where(u => u.subscription.Id == subscription.Id).ToList();

            foreach (var item in mobileService)
            {
                Application.Current.Dispatcher.Invoke(() => MobileServiceList.Add(item.Name));
            }

            if (AutoRefresh)
                GetMobileServiceOnline(subscription);

            Message = String.Empty;
            LoadingRingMessage = false; 
        }

        private void GetMobileServiceOnline(Subscription subscription)
        {

            MobileDbContext ctx = new MobileDbContext();
            var subscriptionDb = ctx.Subscriptions.Where(s => s.Id == subscription.Id).First();
            try
            { 
            var service = Data.Helper.GetMobileServiceInSubscription(subscription.Id.ToString("d"));

            if (service == null)
                return; 

            service.ForEach(u =>
            {
                if (!MobileServiceList.Contains(u.Name))
                {
                    Application.Current.Dispatcher.Invoke(() => MobileServiceList.Add(u.Name));
                    
                    subscriptionDb.MobileServices.Add(new MobileService() { Name = u.Name });
                    
                    try
                    {
                        ctx.SaveChanges();
                        Message = String.Format("Inserted New Mobile Service Reference : {0}", u.Name);
                    }
                    catch(Exception e){
                        Message = String.Format("Error during Mobile Service List Update : {0}", e.Message);
                    }

                }
            });
                }
            catch(Exception e){
                Message = e.Message;
            }
        }
        #endregion

        #region Help Button
        public ICommand HelpAboutCommand
        {
            get { return new Command.DelegateCommand((o)=>{
                    var testc = String.Empty;
                    new View.About().ShowDialog();
                   }
                ); 
            }
        }
        #endregion

        
    }

}

using RSS.Model;
using RSS.Command;
using RSS.Properties;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.IO;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using MahApps.Metro.Controls.Dialogs;

namespace RSS.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDialogCoordinator dialogCoordinator;
        private readonly MetroDialogSettings dialogSettings = new MetroDialogSettings();
        private Timer timer;
        private DateTimeOffset lastUpdate = DateTimeOffset.MinValue;
        
        private ICollection<string> urlList = new ObservableCollection<string>(Settings.Default.URL_LIST.Cast<string>().ToList());
        public ICollection<string> UrlList
        {
            get { return urlList; }
            set
            {
                if (urlList != value)
                {
                    urlList = value;
                    OnPropertyChanged("UrlList");
                }
            }
        }

        public string CurrentUrl
        {
            get { return Settings.Default.CURRENT_URL; }
            set
            {
                if (Settings.Default.CURRENT_URL != value)
                {
                    Settings.Default.CURRENT_URL = value;
                    OnPropertyChanged("CurrentUrl");
                }
            }
        }

        public bool IsProxy 
        {
            get { return Settings.Default.IS_PROXY; }

            set 
            {
                if (Settings.Default.IS_PROXY != value)
                {
                    Settings.Default.IS_PROXY = value;
                    OnPropertyChanged("IsProxy");
                }
            }
        }

        public string ProxyIP 
        {
            get { return Settings.Default.PROXY_IP; }
            set
            {
                if (Settings.Default.PROXY_IP != value)
                {
                    Settings.Default.PROXY_IP = value;
                    OnPropertyChanged("ProxyIP");
                }
            }
        }

        public int ProxyPort 
        {
            get { return Settings.Default.PROXY_PORT; }
            set
            {
                if (Settings.Default.PROXY_PORT != value)
                {
                    Settings.Default.PROXY_PORT = value;
                    OnPropertyChanged("ProxyPort");
                }
            }
        }

        public bool IsAuthentication
        {
            get { return Settings.Default.IS_AUTHENTICATION; }

            set
            {
                if (Settings.Default.IS_AUTHENTICATION != value)
                {
                    Settings.Default.IS_AUTHENTICATION = value;
                    OnPropertyChanged("IsAuthentication");
                }
            }
        }

        public string ProxyUser 
        {
            get { return Settings.Default.PROXY_USER;  }
            set
            {
                if (Settings.Default.PROXY_USER != value)
                {
                    Settings.Default.PROXY_USER = value;
                    OnPropertyChanged("ProxyUser");
                }
            }
        }

        private string title;
        public string Title 
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        private byte[] image;
        public byte[] Image 
        {
            get { return image; }
            set
            {
                if (image != value)
                {
                    image = value;
                    OnPropertyChanged("Image");
                }
            }
        }

        private ICollection<RssFeed> rssFeedList;
        public ICollection<RssFeed> RssFeedList
        {
            get { return rssFeedList; }
            set
            {
                if (rssFeedList != value)
                {
                    rssFeedList = value;
                    OnPropertyChanged("RssFeedList");
                }
            }
        }

        private bool showSettings;
        public bool ShowSettings 
        {
            get { return showSettings; }
            set
            {
                if (showSettings != value)
                {
                    showSettings = value;
                    OnPropertyChanged("ShowSettings");
                }
            }
        }

        private bool isWork;
        public bool IsWork
        {
            get { return isWork; }
            set
            {
                if (isWork != value)
                {
                    isWork = value;
                    OnPropertyChanged("IsWork");
                }
            }
        }

        public ICommand UpdateCommand { get; set; }
        public ICommand LoadSiteCommand { get; set; }
        public ICommand AddRssCommand { get; set; }
        public ICommand DeleteRssCommand { get; set; }
        public ICommand CloseSettingsCommand { get; set; }

        public MainViewModel(IDialogCoordinator coordinator)
        {
            dialogCoordinator = coordinator;
            dialogSettings.AffirmativeButtonText = "ДОБРЕ";
            dialogSettings.NegativeButtonText = "ВІДМІНА";
            UpdateCommand = new BaseCommand(DoUpdateCommand);
            LoadSiteCommand = new BaseCommand(DoLoadSiteCommand);
            AddRssCommand = new BaseCommand(DoAddRssCommand);
            DeleteRssCommand = new BaseCommand(DoDeleteRssCommand, CanDeleteRssCommand);
            CloseSettingsCommand = new BaseCommand(DoCloseSettingsCommand);
            timer = new Timer((obj) => { LoadData(); }, null, 0, 600000);
        }

        private void DoUpdateCommand(object obj)
        {
            ShowSettings = false;
            Properties.Settings.Default.Save();
            Task.Factory.StartNew(LoadData);
        }

        private void DoLoadSiteCommand(object obj)
        {
            Uri temp = obj as Uri;
            if (temp != null)
            {
                Process.Start(new ProcessStartInfo(temp.AbsoluteUri));
            }
        }

        private void DoAddRssCommand(object obj)
        {
            string result = dialogCoordinator.ShowModalInputExternal(this, "Додати RSS", "Введіть нову URL RSS:", dialogSettings);
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Trim();
                if (!UrlList.Contains(result))
                {
                    UrlList.Add(result);
                    Settings.Default.URL_LIST.Add(result);
                    dialogCoordinator.ShowMessageAsync(this, "Повідомлення", "Нову URL RSS додано", MessageDialogStyle.Affirmative, dialogSettings);
                    CurrentUrl = result;
                }
                else
                {
                    dialogCoordinator.ShowMessageAsync(this, "Повідомлення", "Нову URL RSS не було додано, оскільки така URL RSS вже є", MessageDialogStyle.Affirmative, dialogSettings);
                }
            }
            else
            {
                dialogCoordinator.ShowMessageAsync(this, "Повідомлення", "Нову URL RSS не було додано", MessageDialogStyle.Affirmative, dialogSettings);
            }
        }

        private void DoDeleteRssCommand(object obj)
        {
            string temp = CurrentUrl;
            MessageDialogResult result = dialogCoordinator.ShowModalMessageExternal(this, "Повідомлення", "Видалити: " + temp + "?", MessageDialogStyle.AffirmativeAndNegative, dialogSettings);
            if (result == MessageDialogResult.Affirmative)
            {
                UrlList.Remove(temp);
                Settings.Default.URL_LIST.Remove(temp);
                dialogCoordinator.ShowMessageAsync(this, "Повідомлення", "URL RSS " + temp + " було успішно видалено", MessageDialogStyle.Affirmative, dialogSettings);
                if (UrlList.Count > 0)
                {
                    CurrentUrl = UrlList.ElementAt(0);
                }
            }
            else
            {
                dialogCoordinator.ShowMessageAsync(this, "Повідомлення", "URL RSS " + temp + " не було видалено", MessageDialogStyle.Affirmative, dialogSettings);
            }
        }

        private bool CanDeleteRssCommand(object obj)
        {
            return (UrlList.Count > 0);
        }

        private void DoCloseSettingsCommand(object obj)
        {
            ShowSettings = false;
            Settings.Default.Reload();           
            OnPropertyChanged("CurrentUrl");
            OnPropertyChanged("IsProxy");
            OnPropertyChanged("ProxyIP");
            OnPropertyChanged("ProxyPort");
            OnPropertyChanged("IsAuthentication");
            OnPropertyChanged("ProxyUser");
        }

        private void LoadData()
        {
            IsWork = true;
            try
            {
                WebClient client = new WebClient();
                if (IsProxy)
                {
                    WebProxy proxy = new WebProxy(ProxyIP, ProxyPort);
                    if (IsAuthentication)
                    {
                        proxy.Credentials = new NetworkCredential(ProxyUser, Settings.Default.PROXY_PASSWORD);
                    }
                    client.Proxy = proxy;
                }
                MemoryStream stream = new MemoryStream(client.DownloadData(CurrentUrl));
                XmlTextReader reader = new XmlTextReader(stream);
                Rss20FeedFormatter rss = new Rss20FeedFormatter();
                rss.ReadFrom(reader);
                SyndicationFeed feed = rss.Feed;
                if (feed != null && (feed.LastUpdatedTime > lastUpdate || feed.LastUpdatedTime == DateTimeOffset.MinValue) )
                {
                    lastUpdate = feed.LastUpdatedTime;
                    Title = feed.Title.Text;
                    Image = client.DownloadData(feed.ImageUrl);
                    Application.Current.Dispatcher.Invoke(new Action (() =>
                    {
                        RssFeedList = new ObservableCollection<RssFeed>();
                        foreach (SyndicationItem item in feed.Items)
                        {
                            RssFeed rssFeed = new RssFeed(item);
                            RssFeedList.Add(rssFeed);
                        }    
                    }));
                }
            }
            catch (Exception ex)
            {
                try
                {
                    dialogCoordinator.ShowMessageAsync(this, "Помилка", ex.Message, MessageDialogStyle.Affirmative, dialogSettings);
                }
                catch 
                {
                    Thread.Sleep(1000);
                    dialogCoordinator.ShowMessageAsync(this, "Помилка", ex.Message, MessageDialogStyle.Affirmative, dialogSettings);
                }
            }
            IsWork = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
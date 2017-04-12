using RSS.View;
using RSS.ViewModel;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using MahApps.Metro.Controls.Dialogs;

namespace RSS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            MainWindow window = new MainWindow();
            window.DataContext = new MainViewModel(DialogCoordinator.Instance); 
            window.Show();
        }
    }
}
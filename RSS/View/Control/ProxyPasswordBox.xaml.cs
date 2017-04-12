using RSS.Properties;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RSS.View.Control
{
    /// <summary>
    /// Логика взаимодействия для PasswordBox.xaml
    /// </summary>
    public partial class ProxyPasswordBox : UserControl
    {      
        public ProxyPasswordBox()
        {
            InitializeComponent();
            PasswordBox.Password = Settings.Default.PROXY_PASSWORD;
            PasswordBox.PasswordChanged += (obj, ev) => { Settings.Default.PROXY_PASSWORD = PasswordBox.Password; };
        }
    }
}
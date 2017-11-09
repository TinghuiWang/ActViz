using ActViz.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ActViz.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        public Logger appLog = Logger.Instance;

        bool _isLogVisible = false;
        public bool IsLogVisible
        {
            get { return _isLogVisible; }
            set
            {
                SetProperty(ref _isLogVisible, value);
                OnPropertyChanged("IsHorizontalLogVisible");
                OnPropertyChanged("IsVerticalLogVisible");
            }
        }

        bool _isLogHorizontal = true;
        public bool IsLogHorizontal
        {
            get { return _isLogHorizontal; }
            set
            {
                SetProperty(ref _isLogHorizontal, value);
                OnPropertyChanged("IsHorizontalLogVisible");
                OnPropertyChanged("IsVerticalLogVisible");
            }
        }

        public bool IsHorizontalLogVisible { get { return IsLogVisible && _isLogHorizontal; } }
        public bool IsVerticalLogVisible { get { return IsLogVisible && (!_isLogHorizontal); } }

        public ObservableCollection<NavigationViewItem> MainNavMenu = new ObservableCollection<NavigationViewItem>();
        public ObservableCollection<NavigationViewItem> FooterMenu = new ObservableCollection<NavigationViewItem>();
    }
}

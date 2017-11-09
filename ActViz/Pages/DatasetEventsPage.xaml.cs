using ActViz.Models;
using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatasetEventsPage : Page
    {
        DatasetEventsViewModel _viewModel;
        
        public DatasetEventsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _viewModel = new DatasetEventsViewModel(e.Parameter as Dataset);
            base.OnNavigatedTo(e);
        }

        private void btnPrevWeek_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dataListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dataListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {

        }

        private void btnPrevDay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DateSlider_ValueChangeCompleted(object sender, object args)
        {

        }

        private void btnNextDay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNextWeek_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuFilter_ClickAsync(object sender, RoutedEventArgs e)
        {

        }

        private void menuConfig_ClickAsync(object sender, RoutedEventArgs e)
        {

        }

        private void sensorCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void floorplanImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void EventDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {

        }
    }
}

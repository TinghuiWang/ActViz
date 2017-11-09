using ActViz.Models;
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
    public sealed partial class DatasetViewPage : Page
    {
        Dataset _dataset;

        public DatasetViewPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            textBlockTitle.Text = (_dataset == null) ? "" : _dataset.Name;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _dataset = e.Parameter as Dataset;
            MainPage mainPage = (Window.Current.Content as Frame).Content as MainPage;
            mainPage.NavigationViewLoadMenu(new List<NavigationViewItem>()
            {
                new NavigationViewItem()
                {
                    Icon = new FontIcon() { Glyph = "\xE292" },
                    Content = "View Events",
                    Tag = "events"
                },
                new NavigationViewItem()
                {
                    Icon = new FontIcon() { Glyph = "\xE713" },
                    Content = "Configure",
                    Tag = "configure"
                },
                new NavigationViewItem()
                {
                    Icon = new FontIcon() { Glyph = "\xE8BB" },
                    Content = "Close",
                    Tag = "close"
                }
            }, (sender, args) =>
            {
                switch((string)(args.SelectedItem as NavigationViewItem).Tag)
                {
                    case "close":
                        ClosePage();
                        break;
                    case "configure":
                        localFrame.Navigate(typeof(DatasetConfigurePage), _dataset);
                        break;
                    case "events":
                        localFrame.Navigate(typeof(DatasetEventsPage), _dataset);
                        break;
                    default:
                        break;
                }
            });
            base.OnNavigatedTo(e);
        }

        private void ClosePage()
        {
            MainPage mainPage = (Window.Current.Content as Frame).Content as MainPage;
            mainPage.BackToEmpty();
        }
    }
}

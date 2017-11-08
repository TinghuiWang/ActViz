using ActViz.Helpers;
using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ActViz
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MainPageViewModel _viewModel = new MainPageViewModel();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.appLog.Info(this.GetType().ToString(), "MainPage Loaded.");
            CoreApplicationViewTitleBar titleBar = CoreApplication.GetCurrentView().TitleBar;
            titleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            // TODO: Add Application Title
            AppTitle.Margin = new Thickness(CoreApplication.GetCurrentView().TitleBar.SystemOverlayLeftInset + mainNavView.CompactPaneLength + 12, 8, 0, 0);
        }

        // Handles Log Window
        private void navViewListAppLog_ItemClick(object sender, ItemClickEventArgs e)
        {
            NavigationViewList navViewList = sender as NavigationViewList;
            NavigationViewItem item = navViewList.Items[0] as NavigationViewItem;
            _viewModel.IsLogVisible = !_viewModel.IsLogVisible;
            // Change Grid assignment of main frame based on log visibility
            if (!_viewModel.IsLogVisible)
            {
                // Log is not visible, expand main frame to fill the grid.
                Grid.SetRowSpan(mainFrame, 2);
                Grid.SetColumnSpan(mainFrame, 2);
                // Set Visual State
                VisualStateManager.GoToState(item, "Checked", true);
            }
            else
            {
                if (_viewModel.IsLogHorizontal)
                {
                    // Log is horizontally aligned
                    Grid.SetRowSpan(mainFrame, 1);
                    Grid.SetColumnSpan(mainFrame, 2);
                }
                else
                {
                    // Log is vertically aligned
                    Grid.SetColumnSpan(mainFrame, 1);
                    Grid.SetRowSpan(mainFrame, 2);
                }
                VisualStateManager.GoToState(item, "Normal", true);
            }
        }

        #region Logger Window
        private async void btnLogSave_ClickAsync(object sender, RoutedEventArgs e)
        {
            var dialog = new FileSavePicker();
            dialog.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            dialog.FileTypeChoices.Add("Log File", new List<string> { ".log" });
            dialog.SuggestedFileName = "ActViz";
            dialog.DefaultFileExtension = ".log";

            StorageFile file = await dialog.PickSaveFileAsync();
            if (file != null)
            {
                await FileIO.WriteTextAsync(file, _viewModel.appLog.Log);
            }
        }

        private readonly Thickness _logGridHorizontalPadding = new Thickness(0, 15, 0, 0);
        private readonly Thickness _logGridVerticalPadding = new Thickness(15, 0, 0, 0);

        private void cbDebugLog_Unchecked(object sender, RoutedEventArgs e)
        {
            _viewModel.appLog.LogLevel = Logger.LOGGING_INFO;
        }

        private void cbDebugLog_Checked(object sender, RoutedEventArgs e)
        {
            _viewModel.appLog.LogLevel = Logger.LOGGING_DEBUG;
        }

        private void btnHorizontalSplit_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.IsLogHorizontal = true;
            // Apply Horizontal Grid Settings
            // Main frame: Row 0, Column 0, RowSpan 1, ColumnSpan 2
            // Log Text Grid: Row 1, Column 0, RowSpan 1, ColumnSpan 2
            Grid.SetColumnSpan(mainFrame, 2);
            Grid.SetRowSpan(mainFrame, 1);
            Grid.SetColumn(gridLogText, 0);
            Grid.SetRow(gridLogText, 1);
            Grid.SetColumnSpan(gridLogText, 2);
            Grid.SetRowSpan(gridLogText, 1);
            gridLogText.Margin = _logGridHorizontalPadding;
        }

        private void btnVerticalSplit_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.IsLogHorizontal = false;
            // Apply Vertical Grid Settings
            // Main frame: Row 0, Column 0, RowSpan 2, ColumnSpan 1
            // Log Text Grid: Row 0, Column 1, RowSpan 2, ColumnSpan 1
            Grid.SetColumnSpan(mainFrame, 1);
            Grid.SetRowSpan(mainFrame, 2);
            Grid.SetColumn(gridLogText, 1);
            Grid.SetRow(gridLogText, 0);
            Grid.SetColumnSpan(gridLogText, 1);
            Grid.SetRowSpan(gridLogText, 2);
            gridLogText.Margin = _logGridVerticalPadding;
        }
        #endregion

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {

        }

        private void NavigationViewList_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}

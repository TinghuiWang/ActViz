using ActViz.Dialogs;
using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
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
    public sealed partial class DatabaseImportPage : Page
    {
        DatabaseImportViewModel _viewModel = new DatabaseImportViewModel();

        public DatabaseImportPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainPage mainPage = (Window.Current.Content as Frame).Content as MainPage;
            mainPage.NavigationViewLoadMenu(new List<NavigationViewItem>()
            {
                new NavigationViewItem()
                {
                    Icon = new FontIcon() { Glyph = "\xE8BB" },
                    Content = "Close",
                    Tag = "close"
                }
            }, (sender, args) => {
                if((string) (args.SelectedItem as NavigationViewItem).Tag == "close")
                {
                    this.ClosePage();
                }
            });

            base.OnNavigatedTo(e);
        }

        private async void Page_LoadedAsync(object sender, RoutedEventArgs e)
        {
            DatabaseConnectionDialog dialog = new DatabaseConnectionDialog(_viewModel.DatabaseConnection);
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _viewModel.DatabaseConnection.SaveToLocalSettings();
                await _viewModel.UpdateTestBedsAsync();
            }
            else
            {
                ClosePage();
            }
        }

        private void PageBusy(string message)
        {
            ((Window.Current.Content as Frame).Content as MainPage).PageBusy(message);
        }

        private void PageReady()
        {
            ((Window.Current.Content as Frame).Content as MainPage).PageReady();
        }

        private async void btnImportFromTestbed_ClickAsync(object sender, RoutedEventArgs e)
        {
            PageBusy("Loading Selected TestBed from database ...");
            await _viewModel.LoadExsitingSitesAsync();
            await _viewModel.LoadSensorsFromDbAsync();
            _viewModel.DatasetName = _viewModel.TestBedSelected.Name;
            _viewModel.SiteName = _viewModel.TestBedSelected.Name;
            _viewModel.DatasetImportStartDate = _viewModel.TestBedSelected.CreatedTime;
            _viewModel.DatasetImportStopDate = new DateTimeOffset(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
            _viewModel.TestBedSelected.TimeZone.BaseUtcOffset);
            _viewModel.InImportMode = true;
            PageReady();
        }

        private void chkboxCreateNewSite_Checked(object sender, RoutedEventArgs e)
        {
        }

        private async void btnSelectFloorplan_ClickAsync(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".bmp");
            StorageFile floorPlanFile = await filePicker.PickSingleFileAsync();
            if (floorPlanFile != null)
            {
                _viewModel.SiteFloorplan = floorPlanFile.Path;
                _viewModel.SiteFloorplanFile = floorPlanFile;
            }
        }

        private void datePickerStopDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {

        }

        private void timePickerStopTime_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
        }

        private void timePickerStartTime_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
        }

        private void datePickerStartDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
        }

        private async void btnDatasetFinishImport_ClickAsync(object sender, RoutedEventArgs e)
        {
            PageBusy("Creating dataset (and site) from database ...");
            try
            {
                await _viewModel.ImportSelectedDatasetAsync();
                _viewModel.InImportMode = false;
                MessageDialog dlg = new MessageDialog("Dataset " + _viewModel.DatasetName + " imported successfully");
                await dlg.ShowAsync();
            }
            catch (Exception except)
            {
                MessageDialog dialog = new MessageDialog("Error importing selected dataset\n" + except.Message, "Error Importing dataset");
                await dialog.ShowAsync();
            }
            PageReady();
        }

        private void btnDatasetCancelImport_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.InImportMode = false;
        }

        void ClosePage()
        {
            ((Window.Current.Content as Frame).Content as MainPage).BackToEmpty();
        }
    }
}

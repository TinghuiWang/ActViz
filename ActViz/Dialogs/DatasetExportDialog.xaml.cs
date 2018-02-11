using ActViz.Models;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz.Dialogs
{
    public sealed partial class DatasetExportDialog : ContentDialog
    {
        DatasetExportViewModel _viewModel;
        public StorageFolder ExportFolder;
        public DatasetExportViewModel DatasetExportConfiguration
        {
            get { return _viewModel; }
        }

        public DatasetExportDialog(DatasetEventsViewModel datasetEventsView)
        {
            _viewModel = new DatasetExportViewModel(datasetEventsView);
            this.InitializeComponent();
        }

        private async void BtnExportPathSelect_ClickAsync(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                ViewMode = PickerViewMode.List
            };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                tbExportPath.Text = folder.Path;
                IsPrimaryButtonEnabled = true;
                ExportFolder = folder;
            }
        }

        private void datePickerStopDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {

        }

        private void datePickerStartDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {

        }

        private void DatasetExportDialog_ExportButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void DatasetExportDialog_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }
    }
}

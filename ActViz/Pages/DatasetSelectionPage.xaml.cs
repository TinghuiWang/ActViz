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
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatasetSelectionPage : Page
    {
        DatasetSelectionViewModel _viewModel = new DatasetSelectionViewModel();

        public DatasetSelectionPage()
        {
            this.InitializeComponent();
        }

        private void PageBusy(string message)
        {
            ((Window.Current.Content as Frame).Content as MainPage).PageBusy(message);
        }

        private void PageReady()
        {
            ((Window.Current.Content as Frame).Content as MainPage).PageReady();
        }

        private async void Page_LoadedAsync(object sender, RoutedEventArgs e)
        {
            PageBusy("Loading datasets from local database...");
            await _viewModel.LoadFromLocalAsync();
            PageReady();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void BtnLoadDataset_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.DatasetSelected != null)
                ((Window.Current.Content as Frame).Content as MainPage).MainFrameNavigate(typeof(DatasetViewPage), _viewModel.DatasetSelected);
        }

        private void BtnCreateDataset_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void BtnImportDataset_ClickAsync(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Load from storage folder
                Dataset dataset = await Dataset.LoadMetadataFromFolderAsync(folder);
                if (_viewModel.GetDatasetId(dataset) >= 0)
                {
                    // Site exists in the list, ask if it needs to be replaced.
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "Do you want to overwrite?",
                        Content = "Dataset " + dataset.Name + " found in datasets repository. Do you want to overwrite it?",
                        PrimaryButtonText = "Yes",
                        SecondaryButtonText = "No",
                        DefaultButton = ContentDialogButton.Secondary
                    };
                    ContentDialogResult result = await dialog.ShowAsync();
                    if (result != ContentDialogResult.Primary)
                        return;
                }
                PageBusy(string.Format("Importing dataset {0}...", dataset.Name));
                await _viewModel.AddDatasetAsync(dataset);
            }
        }

        private async void BtnExportDataset_ClickAsync(object sender, RoutedEventArgs e)
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
                StorageFolder targetDatasetFolder;
                try
                {
                    targetDatasetFolder = await folder.GetFolderAsync(_viewModel.DatasetSelected.Name);
                    string message = "A folder named " + _viewModel.DatasetSelected.Name + " already existed at " + folder.Path + ". Do you want to overwrite?";
                    var dlg = new MessageDialog(message, "Overwrite Folder?");
                    UICommand yesCmd = new UICommand("Yes");
                    UICommand cancelCmd = new UICommand("Cancel");
                    dlg.Commands.Add(yesCmd);
                    dlg.Commands.Add(cancelCmd);
                    dlg.DefaultCommandIndex = 1;
                    dlg.CancelCommandIndex = 1;
                    var cmd = await dlg.ShowAsync();
                    if (cmd == cancelCmd) return;
                }
                catch (Exception) { }
                targetDatasetFolder = await folder.CreateFolderAsync(_viewModel.DatasetSelected.Name, CreationCollisionOption.ReplaceExisting);
                PageBusy(string.Format("Exporting dataset {0} to folder {1}", _viewModel.DatasetSelected.Name, targetDatasetFolder.Path));
                foreach (var datasetFile in await _viewModel.DatasetSelected.Folder.GetFilesAsync())
                {
                    await datasetFile.CopyAsync(targetDatasetFolder, datasetFile.Name, NameCollisionOption.ReplaceExisting);
                }
                PageReady();
            }
        }

        private void datasetView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void BtnDeleteDataset_ClickAsync(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("Do you want to remove dataset " + _viewModel.DatasetSelected.Name + " permanently?", "Remove Dataset");
            dialog.Commands.Add(new UICommand("Yes"));
            dialog.Commands.Add(new UICommand("No"));
            var result = await dialog.ShowAsync();
            if(result.Label == "Yes")
            {
                await _viewModel.RemoveDatasetAsync(_viewModel.DatasetSelected);
            }
        }
    }
}

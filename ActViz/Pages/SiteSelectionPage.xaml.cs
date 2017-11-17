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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SiteSelectionPage : Page
    {
        SiteSelectionViewModel _viewModel = new SiteSelectionViewModel();

        public SiteSelectionPage()
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
            PageBusy("Loading sites from local database...");
            await _viewModel.LoadFromLocalAsync();
            PageReady();
        }

        private async void BtnExportSite_ClickAsync(object sender, RoutedEventArgs e)
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
                StorageFolder targetSiteFolder;
                try
                {
                    targetSiteFolder = await folder.GetFolderAsync(_viewModel.SiteSelected.Name);
                    string message = "A folder named " + _viewModel.SiteSelected.Name + " already existed at " + folder.Path + ". Do you want to overwrite?";
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
                targetSiteFolder = await folder.CreateFolderAsync(_viewModel.SiteSelected.Name, CreationCollisionOption.ReplaceExisting);
                PageBusy(string.Format("Exporting dataset {0} to folder {1}", _viewModel.SiteSelected.Name, targetSiteFolder.Path));
                foreach (var siteFile in await _viewModel.SiteSelected.Folder.GetFilesAsync())
                {
                    await siteFile.CopyAsync(targetSiteFolder, siteFile.Name, NameCollisionOption.ReplaceExisting);
                }
                PageReady();
            }
        }

        private async void BtnImportSite_ClickAsync(object sender, RoutedEventArgs e)
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
                // Load from storage folder
                Site site = await Site.LoadFromFolderAsync(folder);
                if (_viewModel.GetSiteId(site) >= 0)
                {
                    // Site exists in the list, ask if it needs to be replaced.
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "Do you want to overwrite?",
                        Content = "Site " + site.Name + " found in sites repository. Do you want to overwrite it?",
                        PrimaryButtonText = "Yes",
                        SecondaryButtonText = "No",
                        DefaultButton = ContentDialogButton.Secondary
                    };
                    ContentDialogResult result = await dialog.ShowAsync();
                    if (result != ContentDialogResult.Primary)
                        return;
                }
                PageBusy(string.Format("Importing site {0}...", site.Name));
                await _viewModel.AddSiteAsync(site);
                PageReady();
            }
        }

        private void BtnCreateSite_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnEditSite_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SiteSelected != null)
                ((Window.Current.Content as Frame).Content as MainPage).MainFrameNavigate(typeof(SiteEditPage), _viewModel.SiteSelected);

        }
    }
}

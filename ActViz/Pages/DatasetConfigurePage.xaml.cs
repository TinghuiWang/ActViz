using ActViz.Dialogs;
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
    public sealed partial class DatasetConfigurePage : Page
    {
        DatasetViewModel _viewModel;

        public DatasetConfigurePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _viewModel = new DatasetViewModel(e.Parameter as Dataset);
            base.OnNavigatedTo(e);
        }

        private void PageBusy(string message)
        {
            ((Window.Current.Content as Frame).Content as MainPage).PageBusy(message);
        }

        private void PageReady()
        {
            ((Window.Current.Content as Frame).Content as MainPage).PageReady();
        }

        private async void btnAddActivity_ClickAsync(object sender, RoutedEventArgs e)
        {
            HashSet<string> existingActivityNames = new HashSet<string>();
            foreach (ActivityViewModel activity in _viewModel.ActivityCollectionView)
            {
                existingActivityNames.Add(activity.Name);
            }
            AddActivityDialog dlg = new AddActivityDialog(existingActivityNames)
            {
                Width = Window.Current.Bounds.Width * 0.8
            };
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await _viewModel.AddNewActivityAsync(dlg.ActivityViewModel);
            }
        }

        private async void btnModifyActivity_ClickAsync(object sender, RoutedEventArgs e)
        {
            string oldName = _viewModel.ActivitySelected.Name;
            HashSet<string> existingActivityNames= new HashSet<string>();
            foreach (ActivityViewModel activity in _viewModel.ActivityCollectionView)
            {
                if (activity != _viewModel.ActivitySelected)
                {
                    existingActivityNames.Add(activity.Name);
                }
            }
            EditActivityDialog dlg = new EditActivityDialog(_viewModel.ActivitySelected, existingActivityNames)
            {
                Width = Window.Current.Bounds.Width * 0.8
            };
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {

                PageBusy("Updating residents...");
                if (_viewModel.ActivitySelected.Name != oldName)
                {
                    // Search event and rename previus tags.
                    var fileList = await ((Dataset)_viewModel).Folder.GetFilesAsync();
                    foreach (StorageFile file in fileList)
                    {
                        if (file.FileType == ".csv")
                        {
                            IList<string> fileContent = await FileIO.ReadLinesAsync(file);
                            List<string> newFileContent = new List<string>();
                            foreach (string eventString in fileContent)
                            {
                                newFileContent.Add(eventString.Replace("," + oldName + ",", "," + _viewModel.ActivitySelected.Name + ","));
                            }
                            await FileIO.WriteLinesAsync(file, newFileContent);
                        }
                    }
                }
                await _viewModel.SaveMetadataAsync();
                PageReady();
            }
        }

        private async void btnDeleteActivity_ClickAsync(object sender, RoutedEventArgs e)
        {
            string message = "Do you want to remove activity " + _viewModel.ActivitySelected.Name + "?";
            var dlg = new MessageDialog(message, "Remove Activity");
            dlg.Commands.Add(new UICommand("Yes"));
            dlg.Commands.Add(new UICommand("Cancel"));
            dlg.DefaultCommandIndex = 1;
            dlg.CancelCommandIndex = 1;
            var result = await dlg.ShowAsync();
            if (result.Label == "Yes")
            {
                PageBusy("Remove activity " + _viewModel.ActivitySelected.Name + "...");
                var fileList = await ((Dataset)_viewModel).Folder.GetFilesAsync();
                foreach (StorageFile file in fileList)
                {
                    if (file.FileType == ".csv")
                    {
                        IList<string> fileContent = await FileIO.ReadLinesAsync(file);
                        List<string> newFileContent = new List<string>();
                        foreach (string eventString in fileContent)
                        {
                            newFileContent.Add(eventString.Replace("," + _viewModel.ActivitySelected.Name + ",", ",,"));
                        }
                        await FileIO.WriteLinesAsync(file, newFileContent);
                    }
                }
                await _viewModel.RemoveActivityAsync(_viewModel.ActivitySelected);
                PageReady();
            }
        }

        private async void btnAddResident_ClickAsync(object sender, RoutedEventArgs e)
        {
            HashSet<string> existingResidentNames = new HashSet<string>();
            foreach (ResidentViewModel resident in _viewModel.ResidentCollectionView)
            {
                existingResidentNames.Add(resident.Name);
            }
            AddResidentDialog dlg = new AddResidentDialog(existingResidentNames)
            {
                Width = Window.Current.Bounds.Width * 0.8
            };
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await _viewModel.AddNewResidentAsync(dlg.ResidentViewModel);
            }
        }

        private async void btnModifyResident_ClickAsync(object sender, RoutedEventArgs e)
        {
            string oldName = _viewModel.ResidentSelected.Name;
            HashSet<string> existingResidentNames = new HashSet<string>();
            foreach(ResidentViewModel resident in _viewModel.ResidentCollectionView)
            {
                if(resident != _viewModel.ResidentSelected)
                {
                    existingResidentNames.Add(resident.Name);
                }
            }
            EditResidentDialog dlg = new EditResidentDialog(_viewModel.ResidentSelected, existingResidentNames)
            {
                Width = Window.Current.Bounds.Width * 0.8
            };
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                PageBusy("Updating residents...");
                if(_viewModel.ResidentSelected.Name != oldName)
                {
                    // Search event and rename previus tags.
                    var fileList = await ((Dataset)_viewModel).Folder.GetFilesAsync();
                    foreach(StorageFile file in fileList)
                    {
                        if(file.FileType == ".csv")
                        {
                            IList<string> fileContent = await FileIO.ReadLinesAsync(file);
                            List<string> newFileContent = new List<string>();
                            foreach(string eventString in fileContent)
                            {
                                newFileContent.Add(eventString.Replace("," + oldName + ",", "," + _viewModel.ResidentSelected.Name + ","));
                            }
                            await FileIO.WriteLinesAsync(file, newFileContent);
                        }
                    }
                }
                await _viewModel.SaveMetadataAsync();
                PageReady();
            }
        }

        private async void btnDeleteResident_ClickAsync(object sender, RoutedEventArgs e)
        {
            string message = "Do you want to remove resident " + _viewModel.ResidentSelected.Name + "?";
            var dlg = new MessageDialog(message, "Remove Resident");
            dlg.Commands.Add(new UICommand("Yes"));
            dlg.Commands.Add(new UICommand("Cancel"));
            dlg.DefaultCommandIndex = 1;
            dlg.CancelCommandIndex = 1;
            var result = await dlg.ShowAsync();
            if (result.Label == "Yes")
            {
                PageBusy("Remove resident " + _viewModel.ResidentSelected.Name + "...");
                // Search event and rename previus tags.
                var fileList = await ((Dataset)_viewModel).Folder.GetFilesAsync();
                foreach (StorageFile file in fileList)
                {
                    if (file.FileType == ".csv")
                    {
                        IList<string> fileContent = await FileIO.ReadLinesAsync(file);
                        List<string> newFileContent = new List<string>();
                        foreach (string eventString in fileContent)
                        {
                            newFileContent.Add(eventString.Replace("," + _viewModel.ResidentSelected.Name + ",", ",,"));
                        }
                        await FileIO.WriteLinesAsync(file, newFileContent);
                    }
                }
                await _viewModel.RemoveResidentAsync(_viewModel.ResidentSelected);
                PageReady();
            }
        }
    }
}

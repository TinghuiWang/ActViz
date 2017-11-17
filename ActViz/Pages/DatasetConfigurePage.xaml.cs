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

        private async void btnAddActivity_ClickAsync(object sender, RoutedEventArgs e)
        {
            // TODO: Add additional check for duplicated activity names
            AddActivityDialog dlg = new AddActivityDialog()
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
            EditActivityDialog dlg = new EditActivityDialog(_viewModel.ActivitySelected)
            {
                Width = Window.Current.Bounds.Width * 0.8
            };
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await _viewModel.SaveMetadataAsync();
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
                await _viewModel.RemoveActivityAsync(_viewModel.ActivitySelected);
            }
        }

        private async void btnAddResident_ClickAsync(object sender, RoutedEventArgs e)
        {
            AddResidentDialog dlg = new AddResidentDialog()
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
            EditResidentDialog dlg = new EditResidentDialog(_viewModel.ResidentSelected)
            {
                Width = Window.Current.Bounds.Width * 0.8
            };
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await _viewModel.SaveMetadataAsync();
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
                await _viewModel.RemoveResidentAsync(_viewModel.ResidentSelected);
            }
        }
    }
}

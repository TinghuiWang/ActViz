using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class DatabaseConnectionDialog : ContentDialog
    {
        DatabaseConnectionViewModel _viewModel;

        public DatabaseConnectionDialog(DatabaseConnectionViewModel viewModel)
        {
            _viewModel = viewModel;
            this.InitializeComponent();
        }

        private void PageBusy(string message)
        {
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;
            LoadingControl.IsLoading = true;
            ((Window.Current.Content as Frame).Content as MainPage).PageBusy(message);
        }

        private void PageReady()
        {
            IsPrimaryButtonEnabled = true;
            IsSecondaryButtonEnabled = true;
            LoadingControl.IsLoading = false;
            ((Window.Current.Content as Frame).Content as MainPage).PageReady();
        }

        private async Task<bool> ConnectToDatabase()
        {
            return await Task.Factory.StartNew(() => { return _viewModel.TryDBConnect(); });
        }

        private void DatabaseConnectionDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void DatabaseConnectionDialog_ConnectClickAsync(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();
            PageBusy("Connecting to CASAS Database...");
            args.Cancel = !(await ConnectToDatabase());
            PageReady();
            deferral.Complete();
        }
    }
}

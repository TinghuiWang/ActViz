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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz.Dialogs
{
    public sealed partial class DatasetEventFilterDialog : ContentDialog
    {
        DatasetEventFilterDialogViewModel _viewModel;

        public DatasetEventFilterDialog(EventViewFilter eventViewFilter, List<ResidentViewModel> residents, List<ActivityViewModel> activities)
        {
            _viewModel = new DatasetEventFilterDialogViewModel(eventViewFilter, residents, activities);
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _viewModel.UpdateFilter();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}

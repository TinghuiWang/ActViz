using ActViz.Helpers;
using ActViz.Models;
using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class EditResidentDialog : ContentDialog
    {
        ResidentViewModel _viewModel;
        ObservableCollection<Color> ColorCollection = RuntimeColors.Instance.ColorCollection;
        HashSet<string> _existingResidentNames;

        public EditResidentDialog(ResidentViewModel residentViewModel, HashSet<string> existingResidentNames)
        {
            _viewModel = residentViewModel;
            _existingResidentNames = existingResidentNames;
            if (!ColorCollection.Contains(_viewModel.Color))
                ColorCollection.Add(_viewModel.Color);
            this.InitializeComponent();
        }

        private void EditResidentDialog_SaveClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _viewModel.Color = (Color)comboResidentColor.SelectedItem;
            _viewModel.Name = txtResidentName.Text;
        }

        private void EditResidentDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void comboResidentColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboResidentColor.SelectedItem == null)
            {
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                IsPrimaryButtonEnabled = true;
            }
        }

        private void txtResidentName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtResidentName.Text == "" || _existingResidentNames.Contains(txtResidentName.Text))
            {
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                IsPrimaryButtonEnabled = true;
            }
        }
    }
}

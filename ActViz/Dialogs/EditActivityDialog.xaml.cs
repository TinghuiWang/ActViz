using ActViz.Helpers;
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
    public sealed partial class EditActivityDialog : ContentDialog
    {
        ActivityViewModel _viewModel;
        ObservableCollection<Color> ColorCollection = RuntimeColors.Instance.ColorCollection;

        public EditActivityDialog(ActivityViewModel activityViewModel)
        {
            _viewModel = activityViewModel;
            if (!ColorCollection.Contains(_viewModel.Color))
                ColorCollection.Add(_viewModel.Color);
            this.InitializeComponent();
        }

        private void swIsNoise_Toggled(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonEnabled = true;
            if (swIsNoise.IsOn)
            {
                swIsIgnored.IsOn = false;
            }
        }

        private void swIsIgnored_Toggled(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonEnabled = true;
            if (swIsIgnored.IsOn)
            {
                swIsNoise.IsOn = false;
            }
        }

        private void comboActivityColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboActivityColor.SelectedItem != null)
            {
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                IsPrimaryButtonEnabled = true;
            }
        }

        private void txtActivityName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtActivityName.Text == "")
            {
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                IsPrimaryButtonEnabled = true;
            }
        }

        private void ConfigActivityDialog_SaveClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _viewModel.Name = txtActivityName.Text;
            if (comboActivityColor.SelectedItem != null)
            {
                _viewModel.Color = (Color)comboActivityColor.SelectedItem;
            }
            _viewModel.IsNoise = swIsNoise.IsOn;
            _viewModel.IsIgnored = swIsIgnored.IsOn;
        }

        private void ConfigActivityDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}

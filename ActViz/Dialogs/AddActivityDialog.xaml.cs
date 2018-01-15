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
    public sealed partial class AddActivityDialog : ContentDialog
    {
        ActivityViewModel _activityViewModel = new ActivityViewModel(new Activity());
        public ActivityViewModel ActivityViewModel { get { return _activityViewModel; } }
        ObservableCollection<Color> ColorCollection = RuntimeColors.Instance.ColorCollection;
        HashSet<string> _existingActivityNames;

        public AddActivityDialog(HashSet<string> existingActivityNames)
        {
            _existingActivityNames = existingActivityNames;
            this.InitializeComponent();
        }

        private void AddActivityDialog_AddClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void AddActivityDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void txtActivityName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(txtActivityName.Text == "" || _existingActivityNames.Contains(txtActivityName.Text))
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

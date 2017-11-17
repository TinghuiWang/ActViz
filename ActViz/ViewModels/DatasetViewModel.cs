using ActViz.Helpers;
using ActViz.Models;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.ViewModels
{
    public class DatasetViewModel : ObservableObject<Dataset>
    {
        public string Name
        {
            get { return This.Name; }
            set { SetProperty(This.Name, value, () => This.Name = value); }
        }

        private ObservableCollection<ActivityViewModel> ActivityCollection = new ObservableCollection<ActivityViewModel>();
        private ObservableCollection<ResidentViewModel> ResidentCollection = new ObservableCollection<ResidentViewModel>();
        public AdvancedCollectionView ActivityCollectionView;
        public AdvancedCollectionView ResidentCollectionView;

        private ActivityViewModel _activitySelected;
        public ActivityViewModel ActivitySelected
        {
            get { return _activitySelected; }
            set { SetProperty(ref _activitySelected, value); }
        }

        private bool _isActivitySelected = false;
        public bool IsActivitySelected
        {
            get { return _isActivitySelected; }
            set { SetProperty(ref _isActivitySelected, value); }
        }

        private ResidentViewModel _residentSelected;
        public ResidentViewModel ResidentSelected
        {
            get { return _residentSelected; }
            set { SetProperty(ref _residentSelected, value); }
        }

        private bool _isResidentSelected = false;
        public bool IsResidentSelected
        {
            get { return _isResidentSelected; }
            set { SetProperty(ref _isResidentSelected, value); }
        }

        public DatasetViewModel(Dataset dataset) : base (dataset)
        {
            foreach(Activity activity in dataset.Activities)
            {
                ActivityCollection.Add(new ActivityViewModel(activity));
            }
            foreach(Resident resident in dataset.Residents)
            {
                ResidentCollection.Add(new ResidentViewModel(resident));
            }
            ActivityCollectionView = new AdvancedCollectionView(ActivityCollection);
            ResidentCollectionView = new AdvancedCollectionView(ResidentCollection);
            ActivityCollectionView.SortDescriptions.Add(new SortDescription("Name", SortDirection.Ascending));
            ResidentCollectionView.SortDescriptions.Add(new SortDescription("Name", SortDirection.Ascending));
        }

        internal async Task AddNewActivityAsync(ActivityViewModel activityViewModel)
        {
            ActivityCollectionView.Add(activityViewModel);
            This.Activities.Add(activityViewModel);
            await This.WriteMetadataToFolderAsync();
        }

        internal async Task RemoveActivityAsync(ActivityViewModel activityViewModel)
        {
            ActivityCollection.Remove(activityViewModel);
            This.Activities.Remove(activityViewModel);
            await This.WriteMetadataToFolderAsync();
        }

        internal async Task SaveMetadataAsync()
        {
            await This.WriteMetadataToFolderAsync();
        }

        internal async Task AddNewResidentAsync(ResidentViewModel residentViewModel)
        {
            ResidentCollectionView.Add(residentViewModel);
            This.Residents.Add(residentViewModel);
            await This.WriteMetadataToFolderAsync();
        }

        internal async Task RemoveResidentAsync(ResidentViewModel residentViewModel)
        {
            ResidentCollectionView.Remove(residentViewModel);
            This.Residents.Remove(residentViewModel);
            await This.WriteMetadataToFolderAsync();
        }
    }
}

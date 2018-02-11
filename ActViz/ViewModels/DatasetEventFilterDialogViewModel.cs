using ActViz.Helpers;
using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.ViewModels
{
    class DatasetEventFilterDialogViewModel : ObservableObject
    {
        private EventViewFilter _filter;

        private bool _isSensorStatusFilterEnabled = false;
        public bool IsSensorStatusFilterEnabled
        {
            get { return _isSensorStatusFilterEnabled; }
            set { SetProperty(ref _isSensorStatusFilterEnabled, value); }
        }

        public ObservableCollection<SensorStatusFilterViewModel> SensorStatusFilter = new ObservableCollection<SensorStatusFilterViewModel>();

        private bool _isSensorCategoryFilterEnabled = false;
        public bool IsSensorCategoryFilterEnabled
        {
            get { return _isSensorCategoryFilterEnabled; }
            set { SetProperty(ref _isSensorCategoryFilterEnabled, value); }
        }

        public ObservableCollection<SensorCategoryFilterViewModel> SensorCategoryFilter = new ObservableCollection<SensorCategoryFilterViewModel>();

        private bool _isActivityFilterEnabled = false;
        public bool IsActivityFilterEnabled
        {
            get { return _isActivityFilterEnabled; }
            set { SetProperty(ref _isActivityFilterEnabled, value); }
        }

        private bool _isNullActivityVisible = true;
        public bool IsNullActivityVisible
        {
            get { return _isNullActivityVisible; }
            set { SetProperty(ref _isNullActivityVisible, value); }
        }

        public ObservableCollection<ActivityFilterViewModel> ActivityFilter = new ObservableCollection<ActivityFilterViewModel>();

        private bool _isResidentFilterEnabled = false;
        public bool IsResidentFilterEnabled
        {
            get { return _isResidentFilterEnabled; }
            set { SetProperty(ref _isResidentFilterEnabled, value); }
        }

        private bool _isNullResidentVisible = true;
        public bool IsNullResidentVisible
        {
            get { return _isNullResidentVisible; }
            set { SetProperty(ref _isNullResidentVisible, value); }
        }

        public ObservableCollection<ResidentFilterViewModel> ResidentFilter = new ObservableCollection<ResidentFilterViewModel>();

        public DatasetEventFilterDialogViewModel(EventViewFilter filter, List<ResidentViewModel> residents, List<ActivityViewModel> activities)
        {
            _filter = filter;
            List<string> sensorStatusStrings = new List<string> { "ON", "OFF", "PRESENT", "ABSENT", "OPEN", "CLOSE" };
            foreach (string sensorStatus in sensorStatusStrings)
            {
                SensorStatusFilter.Add(new SensorStatusFilterViewModel { SensorStatus = sensorStatus, IsHidden = filter.SensorStatus.Contains(sensorStatus) });
                if (filter.SensorStatus.Contains(sensorStatus)) IsSensorStatusFilterEnabled = true;
            }
            // Populate sensor type filter
            foreach (SensorCategory sensorCategory in SensorType.SensorCategoryCollection)
            {
                SensorCategoryFilter.Add(new SensorCategoryFilterViewModel { SensorCategory = sensorCategory, IsHidden = filter.SensorCategories.Contains(sensorCategory.Name) });
                if (filter.SensorCategories.Contains(sensorCategory.Name)) IsSensorCategoryFilterEnabled = true;
            }
            // Populate activity filter
            foreach (ActivityViewModel activity in activities)
            {
                ActivityFilter.Add(new ActivityFilterViewModel { Activity = activity, IsHidden = filter.Activities.Contains(activity.Name) });
                if (filter.Activities.Contains(activity.Name)) IsActivityFilterEnabled = true;
            }
            // Populate resident filter
            foreach (ResidentViewModel resident in residents)
            {
                ResidentFilter.Add(new ResidentFilterViewModel { Resident = resident, IsHidden = filter.Residents.Contains(resident.Name) });
                if (filter.Residents.Contains(resident.Name)) IsResidentFilterEnabled = true;
            }
            // For events without activity tag or resident tag
            IsNullActivityVisible = !filter.HideEventsWithoutActivity;
            IsNullResidentVisible = !filter.HideEventsWithoutResident;
            IsActivityFilterEnabled = IsActivityFilterEnabled || filter.HideEventsWithoutActivity;
            IsResidentFilterEnabled = IsResidentFilterEnabled || filter.HideEventsWithoutResident;
        }

        public void UpdateFilter()
        {
            _filter.Activities.Clear();
            _filter.Residents.Clear();
            _filter.SensorStatus.Clear();
            _filter.SensorCategories.Clear();
            if (IsSensorStatusFilterEnabled)
            {
                foreach (SensorStatusFilterViewModel sensorStatusFilterEntry in SensorStatusFilter)
                {
                    if (sensorStatusFilterEntry.IsHidden) _filter.SensorStatus.Add(sensorStatusFilterEntry.SensorStatus);
                }
            }
            if (IsSensorCategoryFilterEnabled)
            {
                foreach (SensorCategoryFilterViewModel sensorCategoryFilterEntry in SensorCategoryFilter)
                {
                    if (sensorCategoryFilterEntry.IsHidden) _filter.SensorCategories.Add(sensorCategoryFilterEntry.SensorCategory.Name);
                }
            }
            if (IsActivityFilterEnabled)
            {
                foreach (ActivityFilterViewModel activityFilterEntry in ActivityFilter)
                {
                    if (activityFilterEntry.IsHidden) _filter.Activities.Add(activityFilterEntry.Activity.Name);
                }
            }
            if (IsResidentFilterEnabled)
            {
                foreach (ResidentFilterViewModel residentFilterEntry in ResidentFilter)
                {
                    if (residentFilterEntry.IsHidden) _filter.Residents.Add(residentFilterEntry.Resident.Name);
                }
            }
            _filter.HideEventsWithoutActivity = !IsNullActivityVisible;
            _filter.HideEventsWithoutResident = !IsNullResidentVisible;
        }
    }

    public class ResidentFilterViewModel : ObservableObject
    {
        public ResidentViewModel Resident { get; set; }

        private bool _isHidden = false;
        public bool IsHidden
        {
            get { return _isHidden; }
            set { SetProperty(ref _isHidden, value, "IsHidden"); }
        }
    }


    public class ActivityFilterViewModel : ObservableObject
    {
        public ActivityViewModel Activity { get; set; }

        private bool _isHidden = false;
        public bool IsHidden
        {
            get { return _isHidden; }
            set { SetProperty(ref _isHidden, value, "IsHidden"); }
        }
    }

    public class SensorCategoryFilterViewModel : ObservableObject
    {
        public SensorCategory SensorCategory { get; set; }

        private bool _isHidden = false;
        public bool IsHidden
        {
            get { return _isHidden; }
            set { SetProperty(ref _isHidden, value, "IsHidden"); }
        }
    }

    public class SensorStatusFilterViewModel : ObservableObject
    {
        private string _sensorStatus;
        public string SensorStatus
        {
            get { return _sensorStatus; }
            set { SetProperty(ref _sensorStatus, value, "SensorStatus"); }
        }

        private bool _isHidden = false;
        public bool IsHidden
        {
            get { return _isHidden; }
            set { SetProperty(ref _isHidden, value, "IsHidden"); }
        }
    }
}

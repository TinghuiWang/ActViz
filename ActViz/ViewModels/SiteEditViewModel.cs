using ActViz.Helpers;
using ActViz.Models;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace ActViz.ViewModels
{
    public class SiteEditViewModel : ObservableObject<Site>
    {
        public String Name
        {
            get { return This.Name; }
            set
            {
                SetProperty(This.Name, value, () => This.Name = value);
                IsSiteChanged = true;
            }
        }

        public ImageSource ImgFloorPlan
        {
            get { return This.ImgFloorPlan; }
        }

        public String FloorPlan
        {
            get { return This.Floorplan; }
        }

        ObservableCollection<SensorViewModel> SensorList;
        public AdvancedCollectionView SensorInView;

        public ObservableCollection<SensorCategory> SensorCategories;

        private SensorCategory _sensorCategorySelected;
        public SensorCategory SensorCategorySelected
        {
            get { return _sensorCategorySelected; }
            set
            {
                SetProperty(ref _sensorCategorySelected, value);
                SensorInView.Refresh();
            }
        }

        private bool _isSensorSelected = false;
        public bool IsSensorSelected
        {
            get { return _isSensorSelected; }
            set { SetProperty(ref _isSensorSelected, value); }
        }

        private SensorViewModel _sensorSelected;
        public SensorViewModel SensorSelected
        {
            get { return _sensorSelected; }
            set
            {
                SetProperty(ref _sensorSelected, value);
                if (_sensorSelected != null)
                    IsSensorSelected = true;
            }
        }

        private bool _isSiteChanged = false;
        public bool IsSiteChanged
        {
            get { return _isSiteChanged; }
            set { SetProperty(ref _isSiteChanged, value); }
        }


        public SiteEditViewModel(Site site) : base(site)
        {
            SensorList = new ObservableCollection<SensorViewModel>();
            foreach (Sensor sensor in site.Sensors)
            {
                SensorList.Add(new SensorViewModel(sensor));
            }
            SensorInView = new AdvancedCollectionView(SensorList);
            SensorCategories = SensorType.SensorCategoryCollection;
            SensorInView.Filter = x => FilterSensorByCategory((SensorViewModel)x);
            SensorInView.SortDescriptions.Add(new SortDescription("Name", SortDirection.Ascending));
        }

        private bool FilterSensorByCategory(SensorViewModel x)
        {
            if (SensorCategorySelected != null)
                return x.SensorCategories.Contains(SensorCategorySelected.Name);
            else
                return true;
        }

        public void AddSensor(Sensor sensor)
        {
            if (sensor.Name != "" && This.GetSensor(sensor.Name) == null)
            {
                This.AddSensor(sensor);
                SensorList.Add(new SensorViewModel(sensor));
            }
        }

        public void RemoveSensor(SensorViewModel sensorViewModel)
        {
            This.Sensors.Remove(sensorViewModel);
            SensorList.Remove(sensorViewModel);
            IsSiteChanged = true;
        }
        
        public async Task WriteBackToFolderAsync()
        {
            await This.WriteToFolderAsync();
            IsSiteChanged = false;
        }
    }
}

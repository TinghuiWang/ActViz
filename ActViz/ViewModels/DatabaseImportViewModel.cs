using ActViz.Helpers;
using ActViz.Services;
using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;

namespace ActViz.ViewModels
{
    public class DatabaseImportViewModel : ObservableObject
    {
        private CasasDatabaseService casasDatabaseService = new CasasDatabaseService();
        public DatabaseConnectionViewModel DatabaseConnection;

        public ObservableCollection<TestBedViewModel> TestBedsList = new ObservableCollection<TestBedViewModel>();


        public DatabaseImportViewModel() : base ()
        {
            DatabaseConnection = new DatabaseConnectionViewModel(casasDatabaseService);
        }

        private TestBedViewModel _testbedSelected;
        public TestBedViewModel TestBedSelected
        {
            get { return _testbedSelected; }
            set
            {
                SetProperty(ref _testbedSelected, value);
                IsTestBedSelected = (value != null);
            }
        }

        private bool _inImportMode = false;
        public bool InImportMode
        {
            get { return _inImportMode; }
            set
            {
                SetProperty(ref _inImportMode, value);
                OnPropertyChanged("IsImportBtnAllowed");
            }
        }

        private bool _isTestBedSelected = false;
        public bool IsTestBedSelected
        {
            get { return _isTestBedSelected; }
            set
            {
                SetProperty(ref _isTestBedSelected, value);
                OnPropertyChanged("IsImportBtnAllowed");
            }
        }

        public bool IsImportBtnAllowed
        {
            get { return _isTestBedSelected && !_inImportMode; }
        }

        public async Task UpdateTestBedsAsync()
        {
            var testBedList = await Task.Factory.StartNew(() => { return casasDatabaseService.GetTestBedsInfo(); });
            testBedList.Sort(delegate (TestBedViewModel x, TestBedViewModel y)
            {
                return x.Name.CompareTo(y.Name);
            });
            foreach (TestBedViewModel testbed in testBedList)
            {
                TestBedsList.Add(testbed);
            }
        }

        private string _datasetName;
        public string DatasetName
        {
            get { return _datasetName; }
            set { SetProperty(ref _datasetName, value); }
        }

        private DateTimeOffset _datasetImportStartDate;
        public DateTimeOffset DatasetImportStartDate
        {
            get
            {
                int dateIntervalInDays = (_datasetImportStartDateDateTimeProxy.Date - _datasetImportStartDate.Date).Days;
                double timeIntervalInSeconds = (_datasetImportStartDateTimeSpanProxy - _datasetImportStartDate.TimeOfDay).TotalSeconds;
                _datasetImportStartDate = _datasetImportStartDate.AddDays(dateIntervalInDays);
                _datasetImportStartDate = _datasetImportStartDate.AddSeconds(timeIntervalInSeconds);
                return _datasetImportStartDate;
            }
            set
            {
                _datasetImportStartDate = value;
                DatasetImportStartDateDateTimeProxy = _datasetImportStartDate.Date;
                DatasetImportStartDateTimeSpanProxy = _datasetImportStartDate.TimeOfDay;
            }
        }

        private DateTimeOffset _datasetImportStartDateDateTimeProxy;
        public DateTimeOffset DatasetImportStartDateDateTimeProxy
        {
            get { return _datasetImportStartDateDateTimeProxy; }
            set { SetProperty(ref _datasetImportStartDateDateTimeProxy, value); }
        }

        private TimeSpan _datasetImportStartDateTimeSpanProxy;
        public TimeSpan DatasetImportStartDateTimeSpanProxy
        {
            get { return _datasetImportStartDateTimeSpanProxy; }
            set { SetProperty(ref _datasetImportStartDateTimeSpanProxy, value); }
        }

        private DateTimeOffset _datasetImportStopDate;
        public DateTimeOffset DatasetImportStopDate
        {
            get
            {
                int dateIntervalInDays = (_datasetImportStopDateDateTimeProxy.Date - _datasetImportStopDate.Date).Days;
                double timeIntervalInSeconds = (_datasetImportStopDateTimeSpanProxy - _datasetImportStopDate.TimeOfDay).TotalSeconds;
                _datasetImportStopDate = _datasetImportStopDate.AddDays(dateIntervalInDays);
                _datasetImportStopDate = _datasetImportStopDate.AddSeconds(timeIntervalInSeconds);
                return _datasetImportStopDate;
            }
            set
            {
                _datasetImportStopDate = value;
                DatasetImportStopDateDateTimeProxy = _datasetImportStopDate.Date;
                DatasetImportStopDateTimeSpanProxy = _datasetImportStopDate.TimeOfDay;
            }
        }

        private DateTimeOffset _datasetImportStopDateDateTimeProxy;
        public DateTimeOffset DatasetImportStopDateDateTimeProxy
        {
            get { return _datasetImportStopDateDateTimeProxy; }
            set { SetProperty(ref _datasetImportStopDateDateTimeProxy, value); }
        }

        private TimeSpan _datasetImportStopDateTimeSpanProxy;
        public TimeSpan DatasetImportStopDateTimeSpanProxy
        {
            get { return _datasetImportStopDateTimeSpanProxy; }
            set { SetProperty(ref _datasetImportStopDateTimeSpanProxy, value); }
        }

        public ObservableCollection<Site> existingSiteCollection = new ObservableCollection<Site>();

        private bool _createNewSite = false;
        public bool CreateNewSite
        {
            get { return _createNewSite; }
            set { SetProperty(ref _createNewSite, value); }
        }

        private string _siteName;
        public string SiteName
        {
            get { return _siteName; }
            set { SetProperty(ref _siteName, value); }
        }

        private string _siteFloorplan;
        public string SiteFloorplan
        {
            get { return _siteFloorplan; }
            set { SetProperty(ref _siteFloorplan, value); }
        }

        public StorageFile SiteFloorplanFile { get; set; }

        public ObservableCollection<String> SensorTypeList = new ObservableCollection<string>();
        public ObservableCollection<String> SensorList = new ObservableCollection<string>();

        public void LoadStartAndStopTimeFromDb()
        {
            Tuple<DateTime, DateTime> startStopTime = casasDatabaseService.GetStartStopDateTime(TestBedSelected);
            DatasetImportStartDate = new DateTimeOffset(DateTime.SpecifyKind(startStopTime.Item1, DateTimeKind.Unspecified), TestBedSelected.TimeZone.BaseUtcOffset);
            DatasetImportStopDate = new DateTimeOffset(DateTime.SpecifyKind(startStopTime.Item2, DateTimeKind.Unspecified), TestBedSelected.TimeZone.BaseUtcOffset);
        }

        public async Task LoadSensorsFromDbAsync()
        {
            var sensorTargetList = await Task.Factory.StartNew(() => { return casasDatabaseService.GetDistinctSensorTargetList(TestBedSelected); });
            foreach (string sensorTarget in sensorTargetList)
            {
                SensorList.Add(sensorTarget);
            }
            var sensorTypeList = await Task.Factory.StartNew(() => { return casasDatabaseService.GetSensorTypeList(TestBedSelected); });
            foreach (string sensorType in sensorTypeList)
            {
                SensorTypeList.Add(sensorType);
            }
        }

        public async Task ImportSelectedDatasetAsync()
        {
            if (CreateNewSite) await CreateNewSiteFromTestbed();
            // TODO: Exception Handling
            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("datasets", CreationCollisionOption.OpenIfExists);
            StorageFolder datasetFolder = await folder.CreateFolderAsync(DatasetName, CreationCollisionOption.ReplaceExisting);
            Dataset dataset = new Dataset
            {
                Name = DatasetName,
                SiteName = SiteName,
                Residents = new List<Resident>(),
                Activities = new List<Activity>(),
                Folder = datasetFolder
            };
            await dataset.WriteMetadataToFolderAsync();
            var eventTuple = casasDatabaseService.GetSensorEvents(TestBedSelected, DatasetImportStartDate, DatasetImportStopDate);
            StorageFile eventFile = await dataset.Folder.CreateFileAsync("events.csv", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteLinesAsync(eventFile, eventTuple.Item1);
            eventFile = await dataset.Folder.CreateFileAsync("temperature.csv", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteLinesAsync(eventFile, eventTuple.Item2);
            eventFile = await dataset.Folder.CreateFileAsync("light.csv", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteLinesAsync(eventFile, eventTuple.Item3);
            eventFile = await dataset.Folder.CreateFileAsync("radio.csv", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteLinesAsync(eventFile, eventTuple.Item4);
            eventFile = await dataset.Folder.CreateFileAsync("other.csv", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteLinesAsync(eventFile, eventTuple.Item5);
        }

        public async Task CreateNewSiteFromTestbed()
        {
            // TODO: Exception Handling
            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("sites", CreationCollisionOption.OpenIfExists);
            StorageFolder siteFolder = await folder.CreateFolderAsync(SiteName, CreationCollisionOption.ReplaceExisting);
            Site site = new Site
            {
                Folder = siteFolder,
                Name = SiteName,
                Floorplan = Path.GetFileName(SiteFloorplan),
                Sensors = casasDatabaseService.GetSensors(TestBedSelected),
                TimeZone = TestBedSelected.TimeZone
            };
            await SiteFloorplanFile.CopyAsync(siteFolder, SiteFloorplanFile.Name, NameCollisionOption.ReplaceExisting);
            await site.WriteToFolderAsync();
        }

        public async Task LoadExsitingSitesAsync()
        {
            existingSiteCollection.Clear();
            List<Site> siteList = await LocalMetadataService.LoadSitesAsync();
            foreach(Site site in siteList)
            {
                existingSiteCollection.Add(site);
            }
        }
    }
}

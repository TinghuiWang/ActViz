using ActViz.Helpers;
using ActViz.Models;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;

namespace ActViz.ViewModels
{
    public class DatasetEventsViewModel : ObservableObject
    {
        Logger appLog = Logger.Instance;

        // Some constants
        private readonly int iExpandUnit = 10;

        // SensorViewModelDictionary for fast sensorViewModel Lookup
        Dictionary<string, SensorViewModel> _sensorViewModelDict = new Dictionary<string, SensorViewModel>();
        HashSet<SensorViewModel> _activeSensors = new HashSet<SensorViewModel>();
        public HashSet<SensorViewModel> ActiveSensors { get { return _activeSensors; } }
        // ActivityViewModelDictionary for fast activityViewModel Lookup
        Dictionary<string, ActivityViewModel> _activityViewModelDict = new Dictionary<string, ActivityViewModel>();
        public List<ActivityViewModel> Activities = new List<ActivityViewModel>();
        // ResidentViewModelDictionary for fast residentViewModel Lookup
        Dictionary<string, ResidentViewModel> _residentViewModelDict = new Dictionary<string, ResidentViewModel>();
        public List<ResidentViewModel> Residents = new List<ResidentViewModel>();

        private Dataset _dataset;
        public Dataset Dataset
        {
            get { return _dataset; }
            set { SetProperty(ref _dataset, value); }
        }

        public EventViewFilter EventViewFilter { get; set; }

        List<string> _eventStringList = new List<string>();
        public List<string> AllEventsStringList { get { return _eventStringList; } }
        ObservableCollection<SensorEventViewModel> _allEventsInView = new ObservableCollection<SensorEventViewModel>();
        public AdvancedCollectionView EventsInView;
        private Dictionary<DateTimeOffset, EventOffset> _dictDateEvents = new Dictionary<DateTimeOffset, EventOffset>();
        List<HashSet<string>> _activeSensorsList = new List<HashSet<string>>();

        private SensorEventViewModel _selectedSensorEvent;
        public SensorEventViewModel SelectedSensorEvent
        {
            get { return _selectedSensorEvent; }
            set { SetProperty(ref _selectedSensorEvent, value); }
        }

        private DateTimeOffset _firstEventDate;
        public DateTimeOffset FirstEventDate
        {
            get { return _firstEventDate; }
            set { SetProperty(ref _firstEventDate, value); }
        }

        private DateTimeOffset _lastEventDate;
        public DateTimeOffset LastEventDate
        {
            get { return _lastEventDate; }
            set { SetProperty(ref _lastEventDate, value); }
        }

        private DateTimeOffset _currentDate;
        public DateTimeOffset CurrentDate
        {
            get { return _currentDate; }
            set { SetProperty(ref _currentDate, value); }
        }

        private bool _isEventsModified = false;
        public bool IsEventsModified
        {
            get { return _isEventsModified; }
            set { SetProperty(ref _isEventsModified, value, "IsEventsModified"); }
        }

        private bool _isDatasetModified = false;
        public bool IsDatasetModified
        {
            get { return _isDatasetModified; }
            set { SetProperty(ref _isDatasetModified, value); }
        }

        private int _numEventsInView = 0;
        public int NumEventsInView
        {
            get { return _numEventsInView; }
            set { SetProperty(ref _numEventsInView, value); }
        }

        public int LoadedEventsCount { get { return _eventStringList.Count; } }

        private readonly static HashSet<string> SensorStatusStartToken = new HashSet<string> { "ON", "PRESENT", "OPEN" };
        private readonly static HashSet<string> SensorStatusStopToken = new HashSet<string> { "OFF", "ABSENT", "CLOSE" };

        private Dictionary<string, int> _lastFiredSensorStat = new Dictionary<string, int>();
        public Dictionary<string, int> SensorLastFireStat
        {
            get { return _lastFiredSensorStat; }
        }
        public List<SensorPastInfo> SensorPastInfoList = new List<SensorPastInfo>();
        private int _lastSelectedEventIndex = 0;

        private CoreDispatcher dispatcher;

        private readonly List<string> activeSensorCategories = new List<string>
        {
            "Motion", "Door", "Item", "Light"
        };

        public DatasetEventsViewModel(Dataset dataset)
        {
            Dataset = dataset;
            EventViewFilter = new EventViewFilter();
            EventsInView = new AdvancedCollectionView(_allEventsInView);
            EventsInView.Filter = x => !FilterEvent((SensorEventViewModel)x);
            // Populate Sensor, Resident and Activity Lookup Dictionary
            foreach (Sensor sensor in dataset.Site.Sensors)
            {
                SensorViewModel sensorViewModel = new SensorViewModel(sensor);
                _sensorViewModelDict.Add(sensor.Name, sensorViewModel);
                //if (sensorViewModel.SensorCategories.Intersect(activeSensorCategories).Count() > 0)
                //{
                //    _activeSensors.Add(sensorViewModel);
                //}
            }
            foreach (Activity activity in dataset.Activities)
            {
                ActivityViewModel activityViewModel = new ActivityViewModel(activity);
                _activityViewModelDict.Add(activity.Name, activityViewModel);
                Activities.Add(activityViewModel);
            }
            Activities.Sort(delegate (ActivityViewModel x, ActivityViewModel y)
            {
                return x.Name.CompareTo(y.Name);
            });
            foreach (Resident resident in dataset.Residents)
            {
                ResidentViewModel residentViewModel = new ResidentViewModel(resident);
                _residentViewModelDict.Add(resident.Name, residentViewModel);
                Residents.Add(residentViewModel);
            }
            Residents.Sort(delegate (ResidentViewModel x, ResidentViewModel y)
            {
                return x.Name.CompareTo(y.Name);
            });
            dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
        }

        internal async Task SaveEventsAsync()
        {
            StorageFile eventFile = await Dataset.Folder.CreateFileAsync("events.csv", CreationCollisionOption.ReplaceExisting);
            using (var outputStream = await eventFile.OpenAsync(FileAccessMode.ReadWrite))
            using (var classicStream = outputStream.AsStreamForWrite())
            using (var streamWriter = new StreamWriter(classicStream))
            {
                foreach (string eventString in _eventStringList)
                    streamWriter.WriteLine(eventString);
            }
            if (IsDatasetModified)
            {
                await Dataset.WriteMetadataToFolderAsync();
            }
        }

        internal void TagResident(string tag, SensorEventViewModel sensorEvent)
        {
            sensorEvent.Resident = GetResidentByName(tag);
            int stringIndex = _dictDateEvents[CurrentDate].Offset + _allEventsInView.IndexOf(sensorEvent);
            _eventStringList[stringIndex] = sensorEvent.ToString();
        }

        internal void TagResident(string tag, List<SensorEventViewModel> selectedEvents)
        {
            foreach (SensorEventViewModel sensorEvent in selectedEvents)
            {
                if (sensorEvent.Resident.Name != tag)
                {
                    TagResident(tag, sensorEvent);
                    SensorEventViewModel complementaryEvent = FindComplementaryEvent(sensorEvent);
                    if (complementaryEvent != null)
                        TagResident(tag, complementaryEvent);
                }
            }
            IsEventsModified = true;
        }

        internal void TagAllResidents(string tag)
        {
            for (int idEvent = 0; idEvent < _eventStringList.Count; idEvent++)
            {
                string strCurrentEvent = _eventStringList[idEvent];
                SensorEventViewModel sensorEvent = ParseSensorEventFromString(strCurrentEvent);
                sensorEvent.Resident = GetResidentByName(tag);
                _eventStringList[idEvent] = sensorEvent.ToString();
            }
            IsEventsModified = true;
        }

        internal void TagResidentOfDay(string tag)
        {
            foreach (SensorEventViewModel sensorEvent in _allEventsInView)
            {
                if (sensorEvent.Resident.Name != tag)
                {
                    TagResident(tag, sensorEvent);
                }
            }
            IsEventsModified = true;
        }

        internal void TagActivity(string tag, SensorEventViewModel sensorEvent)
        {
            sensorEvent.Activity = GetActivityByName(tag);
            int stringIndex = _dictDateEvents[CurrentDate].Offset + _allEventsInView.IndexOf(sensorEvent);
            _eventStringList[stringIndex] = sensorEvent.ToString();
        }

        internal void TagActivity(string tag, List<SensorEventViewModel> selectedEvents)
        {
            foreach (SensorEventViewModel sensorEvent in selectedEvents)
            {
                if (sensorEvent.Activity.Name != tag)
                {
                    TagActivity(tag, sensorEvent);
                    SensorEventViewModel complementaryEvent = FindComplementaryEvent(sensorEvent);
                    if (complementaryEvent != null)
                        TagActivity(tag, complementaryEvent);
                }
            }
            IsEventsModified = true;
        }

        internal void UntagActivitiesOfDay()
        {
            foreach (SensorEventViewModel sensorEvent in _allEventsInView)
            {
                sensorEvent.Activity = ActivityViewModel.NullActivity;
            }
            IsEventsModified = true;
        }

        internal void UntagAllActivities()
        {
            for (int idEvent = 0; idEvent < _eventStringList.Count; idEvent++)
            {
                string strCurrentEvent = _eventStringList[idEvent];
                SensorEventViewModel sensorEvent = ParseSensorEventFromString(strCurrentEvent);
                sensorEvent.Activity = ActivityViewModel.NullActivity;
                _eventStringList[idEvent] = sensorEvent.ToString();
            }
            IsEventsModified = true;
        }

        private SensorEventViewModel FindComplementaryEvent(SensorEventViewModel sensorEvent)
        {
            if (SensorStatusStartToken.Contains(sensorEvent.SensorState))
            {
                // Search forward
                int curIndex = _allEventsInView.IndexOf(sensorEvent);
                for (int i = curIndex + 1; i < _allEventsInView.Count; i++)
                {
                    if ((_allEventsInView[i].TimeTag - sensorEvent.TimeTag).TotalSeconds > 3600) break;
                    if (_allEventsInView[i].Sensor == sensorEvent.Sensor)
                        if (SensorStatusStopToken.Contains(_allEventsInView[i].SensorState))
                            return _allEventsInView[i];
                        else
                            break;
                }
            }
            else if (SensorStatusStopToken.Contains(sensorEvent.SensorState))
            {
                // Search backward
                int curIndex = _allEventsInView.IndexOf(sensorEvent);
                for (int i = curIndex - 1; i >= 0; i--)
                {
                    if ((sensorEvent.TimeTag - _allEventsInView[i].TimeTag).TotalSeconds > 3600) break;
                    if (_allEventsInView[i].Sensor == sensorEvent.Sensor)
                        if (SensorStatusStartToken.Contains(_allEventsInView[i].SensorState))
                            return _allEventsInView[i];
                        else
                            break;
                }
            }
            // Do not have/find compllementary pair, return null
            return null;
        }

        /// <summary>
        /// Apply static filters such ad null activities, null residents, sensor status or sensor categories on sensor event.
        /// </summary>
        /// <param name="sensorEvent"></param>
        /// <returns>true if the sensor event is hidden</returns>
        private bool ApplyStaticFilter(SensorEventViewModel sensorEvent)
        {
            if (EventViewFilter.HideEventsWithoutActivity)
            {
                if ((Activity)sensorEvent.Activity == Activity.NullActivity) return true;
            }
            if (EventViewFilter.HideEventsWithoutResident)
            {
                if ((Resident)sensorEvent.Resident == Resident.NullResident) return true;
            }
            if (EventViewFilter.SensorStatus.Contains(sensorEvent.SensorState)) return true;
            if (EventViewFilter.Residents.Contains(sensorEvent.Resident.Name)) return true;
            //foreach (string activityName in EventViewFilter.Activities)
            //{
            //    if (sensorEvent.Activity.Name == activityName) return true;
            //}
            foreach (string sensorCategory in EventViewFilter.SensorCategories)
            {
                if (sensorEvent.Sensor.SensorCategories.Contains(sensorCategory)) return true;
            }
            return false;
        }

        private bool ApplyActivityFilter(SensorEventViewModel sensorEvent)
        {
            if(EventViewFilter.IsActivityFilterEnabled && EventViewFilter.Activities.Count > 0)
            {
                if (EventViewFilter.Activities.Contains(sensorEvent.Activity.Name))
                    return true;
                return sensorEvent.Activity == ActivityViewModel.NullActivity;
            }
            else
            {
                return false;
            }
        }

        private void ExpandFilteredEvents(int idEvent)
        {
            int i = idEvent - 1;
            int count = 0;
            // Check iExpandUnit events above
            while(i >= 0 && count < iExpandUnit)
            {
                // Event already in view. Remove seg end tag or seg begin tag
                if (_allEventsInView[i].IsActivityFiltered || _allEventsInView[i].IsExpandedInView)
                {
                    _allEventsInView[i].IsEndOfSegment = false;
                    break;
                }
                if (_allEventsInView[i].IsFiltered)
                {
                    _allEventsInView[i].IsExpandedInView = true;
                    count++;
                }
                i--;
            }
            // If i reaches end and does not equal to 0, It is a start of a segment
            i++;
            if(i != 0 && count == iExpandUnit)
            {
                _allEventsInView[i].IsStartOfSegment = true;
            }
            // Check the subsequent events
            i = idEvent + 1;
            count = 0;
            while (i < _allEventsInView.Count && count < iExpandUnit)
            {
                // Event already in view. Remove seg end tag or seg begin tag
                if (_allEventsInView[i].IsActivityFiltered || _allEventsInView[i].IsExpandedInView)
                {
                    break;
                }
                if (_allEventsInView[i].IsFiltered)
                {
                    _allEventsInView[i].IsExpandedInView = true;
                    count++;
                }
                i++;
            }
            // If i reaches end and does not equal to end of all events, it is marked as end of a segment
            if(i != _allEventsInView.Count && count == iExpandUnit)
            {
                i--;
                _allEventsInView[i].IsEndOfSegment = true;
            }
        }

        private void ApplyFilter()
        {
            bool IsActivityFilterEnabled = EventViewFilter.IsActivityFilterEnabled;
            // Apply filter to all events
            for(int idEvent = 0; idEvent < _allEventsInView.Count; idEvent ++)
            {
                SensorEventViewModel sensorEvent = _allEventsInView[idEvent];
                sensorEvent.IsFiltered = !ApplyStaticFilter(sensorEvent);
                sensorEvent.IsActivityFiltered = !ApplyActivityFilter(sensorEvent);
                sensorEvent.IsExpandedInView = false;
                sensorEvent.IsStartOfSegment = false;
                sensorEvent.IsEndOfSegment = false;
            }
            if (IsActivityFilterEnabled)
            {
                for (int idEvent = 0; idEvent < _allEventsInView.Count; idEvent++)
                {
                    SensorEventViewModel sensorEvent = _allEventsInView[idEvent];
                    if (sensorEvent.IsActivityFiltered)
                    {
                        ExpandFilteredEvents(idEvent);
                    }
                }
            }
            EventsInView.Refresh();
        }

        private bool FilterEvent(SensorEventViewModel sensorEvent)
        {
            /* Looking for IsFiltered and IsExpandedInView property */
            return !((sensorEvent.IsFiltered && sensorEvent.IsActivityFiltered) || sensorEvent.IsExpandedInView);
        }

        internal void FilterExpand(SensorEventViewModel sensorEvent)
        {
            int idEvent = _allEventsInView.IndexOf(sensorEvent);
            if (idEvent < 0) return;
            int count = 0;
            if(sensorEvent.IsStartOfSegment)
            {
                sensorEvent.IsStartOfSegment = false;
                idEvent--;
                while(idEvent >= 0 && count < iExpandUnit)
                {
                    if(_allEventsInView[idEvent].IsActivityFiltered || _allEventsInView[idEvent].IsExpandedInView)
                    {
                        _allEventsInView[idEvent].IsEndOfSegment = false;
                        break;
                    }
                    if(_allEventsInView[idEvent].IsFiltered)
                    {
                        _allEventsInView[idEvent].IsExpandedInView = true;
                        count++;
                    }
                    idEvent--;
                }
                idEvent++;
                if(count == iExpandUnit && idEvent > 0)
                {
                    _allEventsInView[idEvent].IsStartOfSegment = true;
                }
            }
            else
            {
                sensorEvent.IsEndOfSegment = false;
                idEvent++;
                while (idEvent < _allEventsInView.Count && count < iExpandUnit)
                {
                    if (_allEventsInView[idEvent].IsActivityFiltered || _allEventsInView[idEvent].IsExpandedInView)
                    {
                        _allEventsInView[idEvent].IsStartOfSegment = false;
                        break;
                    }
                    if (_allEventsInView[idEvent].IsFiltered)
                    {
                        _allEventsInView[idEvent].IsExpandedInView = true;
                        count++;
                    }
                    idEvent++;
                }
                idEvent--;
                if (count == iExpandUnit && idEvent < _allEventsInView.Count)
                {
                    _allEventsInView[idEvent].IsEndOfSegment = true;
                }
            }
            EventsInView.Refresh();
        }

        private DateTimeOffset GetNextDay(DateTimeOffset date)
        {
            DateTime localNextDay = date.Date.AddDays(1);
            return new DateTimeOffset(localNextDay, Dataset.Site.TimeZone.GetUtcOffset(localNextDay));
        }

        public async Task LoadDataAsync()
        {
            appLog.Info(GetType().Name, "Loading events...");
            // Local parameters
            string[] tokenList;
            string[] oldTokenList = new string[] { };
            // Clear Event List and Dictionary
            _eventStringList.Clear();
            _allEventsInView.Clear();
            _dictDateEvents.Clear();
            // Start Loading Event
            StorageFile eventFile = await Dataset.Folder.GetFileAsync("events.csv");
            using (var inputStream = await eventFile.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                int lineNo = 0;
                DateTimeOffset? previousDate = null;
                DateTimeOffset? previousEventTimeTag = null;
                while (streamReader.Peek() >= 0)
                {
                    string curEventString = streamReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(curEventString)) continue;
                    try
                    {
                        _eventStringList.Add(curEventString);
                        tokenList = curEventString.Split(new char[] { ',' });
                        // Get the Date of the String and add to dictionary
                        DateTimeOffset curEventTimeTag = DateTimeOffset.Parse(tokenList[0]);
                        DateTime eventLocalDate = curEventTimeTag.Date;
                        DateTimeOffset eventDate = new DateTimeOffset(eventLocalDate, Dataset.Site.TimeZone.GetUtcOffset(eventLocalDate));
                        // If it is the start of the dataset, set first event date.
                        if (previousDate == null) FirstEventDate = eventDate;
                        // Append it to dictionary for date event lookup.
                        if (!_dictDateEvents.TryGetValue(eventDate, out EventOffset eventOffset))
                        {
                            // Cannot get value of the timetag, add it to dictionary (Also, fill the gap between date)
                            if (previousDate != null)
                            {
                                // Fill Length of previous Date
                                _dictDateEvents[previousDate.Value].Length = _eventStringList.Count - 1 - _dictDateEvents[previousDate.Value].Offset;
                                for (DateTimeOffset date = GetNextDay(previousDate.Value); date <= eventDate.Date; date = GetNextDay(date))
                                {
                                    _dictDateEvents.Add(date, new EventOffset() { Offset = _eventStringList.Count - 1, Length = 0 });
                                }
                            }
                            else
                            {
                                _dictDateEvents.Add(eventDate, new EventOffset() { Offset = 0, Length = 0 });
                            }
                        }
                        // Test and see if there is time leap
                        if (previousEventTimeTag == null) previousEventTimeTag = curEventTimeTag;
                        var TimeDifference = curEventTimeTag - previousEventTimeTag.Value;
                        if (TimeDifference.TotalDays >= 0.5)
                        {
                            appLog.Warn(this.GetType().Name, string.Format("Time Leap {0} days {1}H {2}M {3}s after {4}", TimeDifference.Days, TimeDifference.Hours,
                                TimeDifference.Minutes, TimeDifference.Seconds, previousEventTimeTag.Value.ToString("G")));
                        }
                        // Update previous dates register
                        previousDate = eventDate;
                        previousEventTimeTag = curEventTimeTag;
                        lineNo++;
                    }
                    catch (Exception e)
                    {
                        appLog.Error(this.GetType().Name, string.Format("Failed at line {0} with error message {1}", lineNo, e.Message));
                        return;
                    }
                }
                _dictDateEvents[previousDate.Value].Length = _eventStringList.Count - 1 - _dictDateEvents[previousDate.Value].Offset;
                LastEventDate = previousDate.Value;
            }
            appLog.Info(this.GetType().Name, string.Format("Finished loading {0} events from dataset.", _eventStringList.Count));
            // Load First Day
            await LoadEventsAsync(_firstEventDate);
        }

        public async Task LoadEventsAsync(DateTimeOffset date, bool forceReload = false)
        {
            // Off states
            HashSet<string> sensorOffSet = new HashSet<string>() { "off", "close", "absent" };
            if (!forceReload && date == CurrentDate) return;
            _allEventsInView.Clear();
            _activeSensorsList.Clear();
            EventOffset eventOffsetTuple;
            if (_dictDateEvents.TryGetValue(date, out eventOffsetTuple))
            {
                int start = eventOffsetTuple.Offset;
                int length = eventOffsetTuple.Length;
                for (int i = start; i < start + length; i++)
                {
                    SensorEventViewModel currentEvent = ParseSensorEventFromString(_eventStringList[i]);
                    _allEventsInView.Add(currentEvent);
                    HashSet<string> currentActiveSensors = (i == start) ? new HashSet<string>() : 
                        new HashSet<string>(_activeSensorsList[i - start - 1]);
                    if(sensorOffSet.Contains(currentEvent.SensorState.ToLower()))
                    {
                        if(currentActiveSensors.Contains(currentEvent.Sensor.Name))
                        {
                            currentActiveSensors.Remove(currentEvent.Sensor.Name);
                        }
                    }
                    else
                    {
                        if(!currentActiveSensors.Contains(currentEvent.Sensor.Name))
                        {
                            currentActiveSensors.Add(currentEvent.Sensor.Name);
                        }
                    }
                    _activeSensorsList.Add(currentActiveSensors);
                }
                InitSensorFireStatus();
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                 {
                     CurrentDate = date;
                     ApplyFilter();
                     NumEventsInView = EventsInView.Count;
                 });
            }
            else
            {
                appLog.Error("Date {0} not found in events.", date.ToString("G"));
            }
        }

        internal Dictionary<ResidentViewModel, List<SensorEventViewModel>> GetPastStepsOfResidents(int maxSeconds = 7200, int maxSteps = 1000)
        {
            Dictionary<ResidentViewModel, List<SensorEventViewModel>> pastStepsOfResidents =
                new Dictionary<ResidentViewModel, List<SensorEventViewModel>>();
            if (SelectedSensorEvent == null) return pastStepsOfResidents;
            Dictionary<ResidentViewModel, HashSet<SensorViewModel>> sensorCheckDictionary =
                new Dictionary<ResidentViewModel, HashSet<SensorViewModel>>();
            foreach (ResidentViewModel resident in Residents)
            {
                sensorCheckDictionary.Add(resident, new HashSet<SensorViewModel>());
                pastStepsOfResidents.Add(resident, new List<SensorEventViewModel>());
            }
            int eventIndex = _allEventsInView.IndexOf(SelectedSensorEvent);
            DateTimeOffset eventTimeTag = SelectedSensorEvent.TimeTag;
            for (int i = eventIndex; i >= 0; i--)
            {
                SensorEventViewModel tempEvent = _allEventsInView[i];
                // If exceeds maxSSeconds
                if ((eventTimeTag - tempEvent.TimeTag).TotalSeconds > maxSeconds) break;
                // If exceeds maxSteps
                if (eventIndex - i > maxSteps) break;
                // Otherwise, Log sensor if resident is labelled.
                if (tempEvent.Resident != ResidentViewModel.NullResident)
                {
                    // TODO Check if Resident does not exist in Dictionary
                    if (!sensorCheckDictionary[tempEvent.Resident].Contains(tempEvent.Sensor))
                    {
                        if (!SensorStatusStopToken.Contains(tempEvent.SensorState))
                        {
                            pastStepsOfResidents[tempEvent.Resident].Add(tempEvent);
                            //sensorCheckDictionary[tempEvent.Resident].Add(tempEvent.Sensor);
                        }
                    }
                }
            }
            return pastStepsOfResidents;
        }

        internal SensorViewModel GetSensorByName(string name)
        {
            if (!_sensorViewModelDict.TryGetValue(name, out SensorViewModel sensorViewModel))
            {
                appLog.Warn(GetType().ToString(),
                    string.Format("Sensor {0} not found. Add temporarily to site.", name));
                Sensor sensor = new Sensor()
                {
                    Name = name,
                    LocX = 0.0,
                    LocY = 0.0,
                    SizeX = 0.003,
                    SizeY = 0.001,
                    Types = new List<string>()
                };
                sensor.Types.Add(SensorType.GuessSensorTypeFromName(name).Name);
                sensorViewModel = new SensorViewModel(sensor);
                _sensorViewModelDict.Add(name, sensorViewModel);
            }
            if (!_activeSensors.Contains(sensorViewModel))
                _activeSensors.Add(sensorViewModel);
            return sensorViewModel;
        }

        internal void RefreshEventsInView()
        {
            ApplyFilter();
            NumEventsInView = EventsInView.Count;
        }

        internal ActivityViewModel GetActivityByName(string name, bool AddIfNotFound = false, bool cascadeIfNotFound = false)
        {
            if (name == "")
            {
                return ActivityViewModel.NullActivity;
            }
            if (!_activityViewModelDict.TryGetValue(name, out ActivityViewModel activityViewModel))
            {
                if (AddIfNotFound)
                {
                    appLog.Warn(GetType().ToString(),
                        string.Format("Activity {0} not found. Add temporarily to dataset.", name));
                    Activity activity;
                    if (cascadeIfNotFound)
                    {
                        activity = Dataset.GetActivity(name, true);
                    }
                    else
                    {
                        activity = new Activity()
                        {
                            Name = name,
                            Color = Colors.Gray.ToString(),
                            IsIgnored = false,
                            IsNoise = false
                        };
                    }
                    activityViewModel = new ActivityViewModel(activity);
                    _activityViewModelDict.Add(name, activityViewModel);
                }
                else
                {
                    appLog.Warn(GetType().ToString(),
                        string.Format("Activity {0} not found. Return Null Activity.", name));
                    activityViewModel = ActivityViewModel.NullActivity;
                }
            }
            return activityViewModel;
        }

        internal ResidentViewModel GetResidentByName(string name, bool AddIfNotFound = false)
        {
            if (name == "") return ResidentViewModel.NullResident;
            if (!_residentViewModelDict.TryGetValue(name, out ResidentViewModel residentViewModel))
            {
                if (AddIfNotFound)
                {
                    appLog.Warn(GetType().ToString(),
                        string.Format("Resident {0} not found. Add temporarily to dataset.", name));
                    residentViewModel = new ResidentViewModel(new Resident()
                    {
                        Name = name,
                        Color = Colors.Gray.ToString()
                    });
                    _residentViewModelDict.Add(name, residentViewModel);
                }
                else
                {
                    appLog.Warn(GetType().ToString(),
                        string.Format("Resident {0} not found. Return Null Resident.", name));
                    residentViewModel = ResidentViewModel.NullResident;
                }
            }
            return residentViewModel;
        }

        public SensorEventViewModel ParseSensorEventFromString(string eventString)
        {
            SensorEventViewModel sensorEvent = new SensorEventViewModel();
            string[] tokenList = eventString.Split(new Char[] { ',' });
            int numToken = tokenList.Count();
            if (numToken < 3)
                throw new ArgumentException("Number of Tokens in Sensor Event String is smaller than 3");
            // First Token: Date, Second Token: Time (with AM/PM), required
            sensorEvent.TimeTag = DateTimeOffset.Parse(tokenList[0]);
            // Third Token: SensorID, required
            sensorEvent.Sensor = GetSensorByName(tokenList[1]);
            // Fourth Token: Status, required
            sensorEvent.SensorState = tokenList[2];
            // Fifth Token: Occupant
            if (numToken > 3 && !string.IsNullOrWhiteSpace(tokenList[3]))
                sensorEvent.Resident = GetResidentByName(tokenList[3], true);
            else
                sensorEvent.Resident = ResidentViewModel.NullResident;
            // Sixth Token: Activity Labels
            if (numToken > 4 && !string.IsNullOrWhiteSpace(tokenList[4]))
                sensorEvent.Activity = GetActivityByName(tokenList[4], true);
            else
                sensorEvent.Activity = ActivityViewModel.NullActivity;
            // The Rest: Comments
            if (numToken > 5)
                sensorEvent.Comments = string.Join(",", tokenList.Skip(5));
            else
                sensorEvent.Comments = "";
            return sensorEvent;
        }

        internal List<SensorPastInfo> GetPastSensorInfo()
        {
            if (_allEventsInView.Count == 0 || SelectedSensorEvent == null) return SensorPastInfoList;
            SensorPastInfoList.Clear();
            List<KeyValuePair<string, int>> sensorPastFiringList = GetSensorFireStatusSorted();
            foreach (KeyValuePair<string, int> entry in sensorPastFiringList)
            {
                DateTimeOffset sensorLastFireTime = (entry.Value >= 0) ? _allEventsInView[entry.Value].TimeTag : _allEventsInView[0].TimeTag;
                SensorPastInfoList.Add(
                    new SensorPastInfo
                    {
                        Name = entry.Key,
                        LastFireTime = sensorLastFireTime,
                        LastFireElapse = SelectedSensorEvent.TimeTag - sensorLastFireTime,
                        IsValid = (entry.Value >= 0),
                        numEventAgo = sensorPastFiringList[0].Value - entry.Value
                    }
                );
            }
            return SensorPastInfoList;
        }

        /// <summary>
        /// Get Sensor Firing Status - so it can be displayed after apply a SORT function on time.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> UpdateSensorFireStatus()
        {
            if (_allEventsInView.Count == 0) return new Dictionary<string, int>();
            int unfilteredIndex = (SelectedSensorEvent == null) ? -1 : _allEventsInView.IndexOf(SelectedSensorEvent);
            if (unfilteredIndex <= 0)
            {
                foreach (string sensorName in _lastFiredSensorStat.Keys.ToList())
                {
                    _lastFiredSensorStat[sensorName] = -1;
                }
                _lastFiredSensorStat[_allEventsInView[0].Sensor.Name] = 0;
            }
            else
            {
                // If Going Forward - Only need to update the difference
                if (unfilteredIndex > _lastSelectedEventIndex)
                {
                    for (int index = unfilteredIndex; index > _lastSelectedEventIndex; index--)
                    {
                        string sensorName = _allEventsInView[index].Sensor.Name;
                        if (_lastFiredSensorStat[sensorName] < index)
                        {
                            _lastFiredSensorStat[sensorName] = index;
                        }
                    }
                }
                if (unfilteredIndex < _lastSelectedEventIndex)
                {
                    // If Going Backward - Update For those with index greater than current selected index
                    HashSet<string> TargetSensorSet = new HashSet<string>();
                    // Find what sensors need update
                    foreach (string sensorName in _lastFiredSensorStat.Keys.ToList())
                    {
                        if (_lastFiredSensorStat[sensorName] > unfilteredIndex)
                        {
                            _lastFiredSensorStat[sensorName] = -1;
                            TargetSensorSet.Add(sensorName);
                        }
                    }
                    // Trace back until all those sensorName are updated
                    for (int index = unfilteredIndex; index >= 0; index--)
                    {
                        string sensorName = _allEventsInView[index].Sensor.Name;
                        if (TargetSensorSet.Contains(sensorName))
                        {
                            _lastFiredSensorStat[sensorName] = index;
                            TargetSensorSet.Remove(sensorName);
                        }
                        if (TargetSensorSet.Count == 0)
                            break;
                    }
                }
            }
            _lastSelectedEventIndex = unfilteredIndex;
            return _lastFiredSensorStat;
        }

        public void InitSensorFireStatus()
        {
            _lastFiredSensorStat.Clear();
            foreach (SensorViewModel sensor in _activeSensors)
            {
                _lastFiredSensorStat.Add(sensor.Name, -1);
            }
            _lastSelectedEventIndex = 0;
        }

        public List<KeyValuePair<string, int>> GetSensorFireStatusSorted()
        {
            UpdateSensorFireStatus();
            var SensorFireStatusSorted = from entry in _lastFiredSensorStat orderby entry.Value descending select entry;
            return SensorFireStatusSorted.ToList();
        }

        public async Task ImportAnnotationAsync(StorageFile annotationFile)
        {
            using (var inputStream = await annotationFile.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                int lineNo = 0;
                int idEvent = 0;
                string[] tokenList;
                // Make sure Other_Activity exists in dataset metadata.
                ActivityViewModel curActivity = GetActivityByName("Other_Activity", true, true);
                string curAnnotationTag = "";
                // Add Other Activity to activity list
                while (streamReader.Peek() >= 0)
                {
                    string curEventString = streamReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(curEventString)) continue;
                    tokenList = curEventString.Split(new char[] {' ', '\t', ','});
                    // Get the Date of the String and add to dictionary
                    DateTime curEventTimeTag = DateTime.Parse(tokenList[0] + ' ' + tokenList[1]);  
                    // Check the activity label of current event
                    if(tokenList.Length > 4 && !string.IsNullOrWhiteSpace(tokenList[4]))
                    {
                        // Process annotation tag
                        string[] subTokenList = tokenList[4].Split(new char[] { '=' });
                        curActivity = GetActivityByName(subTokenList[0], true, true);
                        // In case the ="begin" part is missing in the annotation
                        if(subTokenList.Length < 2)
                        {
                            if(curAnnotationTag == "\"begin\"")
                            {
                                curAnnotationTag = "\"end\"";
                            }
                            else
                            {
                                curAnnotationTag = "\"begin\"";
                            }
                        }
                        else
                        {
                            curAnnotationTag = subTokenList[1];
                        }
                    }
                    // Append Activity
                    while(idEvent < _eventStringList.Count)
                    {
                        // Check datetime of current event
                        SensorEventViewModel sensorEvent = ParseSensorEventFromString(_eventStringList[idEvent]);
                        if(sensorEvent.TimeTag.DateTime <= curEventTimeTag)
                        {
                            sensorEvent.Activity = curActivity;
                            _eventStringList[idEvent] = sensorEvent.ToString();
                            idEvent++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    // If activity end, update annotation token to OtherActivity
                    if(curAnnotationTag == "\"end\"")
                    {
                        curActivity = GetActivityByName("Other_Activity"); ;
                        curAnnotationTag = "";
                    }
                    // Log Token
                    lineNo++;
                }
            }
            this.IsDatasetModified = true;
            this.IsEventsModified = true;
        }
    }

    public class SensorPastInfo
    {
        public string Name { get; set; }
        public int numEventAgo { get; set; }
        public DateTimeOffset LastFireTime { get; set; }
        public TimeSpan LastFireElapse { get; set; }
        public bool IsValid { get; set; }
    }
}

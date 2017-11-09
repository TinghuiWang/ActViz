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

namespace ActViz.ViewModels
{
    public class DatasetEventsViewModel : ObservableObject
    {
        Logger appLog = Logger.Instance;

        private Dataset _dataset;
        public Dataset Dataset
        {
            get { return _dataset; }
            set { SetProperty(ref _dataset, value); }
        }

        public EventViewFilter EventViewFilter { get; set; }

        List<string> _eventStringList = new List<string>();
        ObservableCollection<SensorEvent> _allEventsInView = new ObservableCollection<SensorEvent>();
        public AdvancedCollectionView EventsInView;
        private Dictionary<DateTime, EventOffset> _dictDateEvents = new Dictionary<DateTime, EventOffset>();

        private SensorEvent _selectedSensorEvent;
        public SensorEvent SelectedSensorEvent
        {
            get { return _selectedSensorEvent; }
            set { SetProperty(ref _selectedSensorEvent, value); }
        }

        private DateTime _firstEventDate;
        public DateTime FirstEventDate
        {
            get { return _firstEventDate; }
            set { SetProperty(ref _firstEventDate, value); }
        }

        private DateTime _lastEventDate;
        public DateTime LastEventDate
        {
            get { return _lastEventDate; }
            set { SetProperty(ref _lastEventDate, value); }
        }

        private DateTime _currentDate;
        public DateTime CurrentDate
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

        public DatasetEventsViewModel(Dataset dataset)
        {
            Dataset = dataset;
            EventViewFilter = new EventViewFilter();
            EventsInView = new AdvancedCollectionView(_allEventsInView);
            EventsInView.Filter = x => !FilterEvent((SensorEvent)x);
        }
        
        private bool FilterEvent(SensorEvent sensorEvent)
        {
            foreach (string sensorStatus in EventViewFilter.SensorStatus)
            {
                if (sensorEvent.SensorState == sensorStatus) return true;
            }
            foreach (string activityName in EventViewFilter.Activities)
            {
                if (sensorEvent.Activity.Name == activityName) return true;
            }
            foreach (string residentName in EventViewFilter.Residents)
            {
                if (sensorEvent.Resident.Name == residentName) return true;
            }
            foreach (string sensorCategory in EventViewFilter.SensorCategories)
            {
                if (sensorEvent.Sensor.Types.Contains(sensorCategory)) return true;
            }
            return false;
        }

        public async Task LoadDataAsync()
        {
            appLog.Info(this.GetType().Name, "Loading events...");
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
                DateTime? previousDate = null;
                DateTime? previousEventTimeTag = null;
                while (streamReader.Peek() >= 0)
                {
                    EventOffset eventOffset;
                    string curEventString = streamReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(curEventString)) continue;
                    try
                    {
                        _eventStringList.Add(curEventString);
                        tokenList = curEventString.Split(new char[] { ',' });
                        // Get the Date of the String and add to dictionary
                        DateTime eventDate = Convert.ToDateTime(tokenList[0]);
                        // If it is the start of the dataset, set first event date.
                        if (previousDate == null) FirstEventDate = eventDate;
                        // Append it to dictionary for date event lookup.
                        if (!_dictDateEvents.TryGetValue(eventDate, out eventOffset))
                        {
                            // Cannot get value of the timetag, add it to dictionary (Also, fill the gap between date)
                            if (previousDate != null)
                            {
                                // Fill Length of previous Date
                                _dictDateEvents[previousDate.Value].Length = _eventStringList.Count - 1 - _dictDateEvents[previousDate.Value].Offset;
                                for (DateTime date = previousDate.Value.AddDays(1); date <= eventDate; date = date.AddDays(1))
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
                        DateTime curEventTimeTag = Convert.ToDateTime(tokenList[0] + " " + tokenList[1]);
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
                LastEventDate = previousDate.Value;
            }
            appLog.Info(this.GetType().Name, string.Format("Finished loading {0} events from dataset.", _eventStringList.Count));
            // Load First Day
            LoadEvents(_firstEventDate);
        }

        public void LoadEvents(DateTime date)
        {
            if (date == CurrentDate) return;
            _allEventsInView.Clear();
            EventOffset eventOffsetTuple;
            if (_dictDateEvents.TryGetValue(date, out eventOffsetTuple))
            {
                CurrentDate = date;
                int start = eventOffsetTuple.Offset;
                int length = eventOffsetTuple.Length;
                for (int i = start; i < start + length; i++)
                {
                    _allEventsInView.Add(SensorEvent.FromString(_eventStringList[i], Dataset));
                }
                //InitSensorFireStatus();
            }
            else
            {
                appLog.Error("Date {0} not found in events.", date.ToString("G"));
            }
        }
    }

    public class SensorPastInfo
    {
        public string Name { get; set; }
        public int numEventAgo { get; set; }
        public DateTime LastFireTime { get; set; }
        public TimeSpan LastFireElapse { get; set; }
        public bool IsValid { get; set; }
    }
}

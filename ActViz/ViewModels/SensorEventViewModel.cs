using ActViz.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.ViewModels
{
    public class SensorEventViewModel : ObservableObject
    {
        private DateTimeOffset _timeTag;
        public DateTimeOffset TimeTag
        {
            get { return _timeTag; }
            set { SetProperty(ref _timeTag, value); }
        }

        private SensorViewModel _sensor;
        public SensorViewModel Sensor
        {
            get { return _sensor; }
            set { SetProperty(ref _sensor, value); }
        }

        private string _sensorState;
        public string SensorState
        {
            get { return _sensorState; }
            set { SetProperty(ref _sensorState, value); }
        }

        private ResidentViewModel _resident;
        public ResidentViewModel Resident
        {
            get { return _resident; }
            set { SetProperty(ref _resident, value); }
        }

        private ActivityViewModel _activity;
        public ActivityViewModel Activity
        {
            get { return _activity; }
            set { SetProperty(ref _activity, value); }
        }

        private string _comments;
        public string Comments
        {
            get { return _comments; }
            set { SetProperty(ref _comments, value); }
        }

        /* The following parameters are for filtering the display */
        private bool _isFiltered = true;
        public bool IsFiltered
        {
            get { return _isFiltered; }
            set { SetProperty(ref _isFiltered, value); }
        }

        private bool _isActivityFiltered = true;
        public bool IsActivityFiltered
        {
            get { return _isActivityFiltered; }
            set { SetProperty(ref _isActivityFiltered, value); }
        }

        private bool _isExpandedInView = false;
        public bool IsExpandedInView
        {
            get { return _isExpandedInView; }
            set { SetProperty(ref _isExpandedInView, value); }
        }

        private bool _isStartOfSegment = false;
        public bool IsStartOfSegment
        {
            get { return _isStartOfSegment; }
            set { SetProperty(ref _isStartOfSegment, value); }
        }

        private bool _isEndOfSegment = false;
        public bool IsEndOfSegment
        {
            get { return _isEndOfSegment; }
            set { SetProperty(ref _isEndOfSegment, value); }
        }
        
        public override string ToString()
        {
            return TimeTag.ToString("MM/dd/yyyy HH:mm:ss.ffffff zzz,") + Sensor.Name + "," + SensorState + "," + Resident.Name + "," + Activity.Name + "," + Comments;
        }
    }
}

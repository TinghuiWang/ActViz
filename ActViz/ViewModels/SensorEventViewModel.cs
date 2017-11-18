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

        public override string ToString()
        {
            return TimeTag.ToString("MM/dd/yyyy HH:mm:ss zzz,") + Sensor.Name + "," + SensorState + "," + Resident.Name + "," + Activity.Name + "," + Comments;
        }
    }
}

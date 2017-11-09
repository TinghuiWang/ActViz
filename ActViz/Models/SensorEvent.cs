using ActViz.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.Models
{
    public class SensorEvent : ObservableObject
    {
        private DateTime _timeTag;
        public DateTime TimeTag
        {
            get { return _timeTag; }
            set { SetProperty(ref _timeTag, value, "TimeTag"); }
        }

        private Sensor _sensor;
        public Sensor Sensor
        {
            get { return _sensor; }
            set { SetProperty(ref _sensor, value, "Sensor"); }
        }

        private string _sensorState;
        public string SensorState
        {
            get { return _sensorState; }
            set { SetProperty(ref _sensorState, value, "SensorState"); }
        }

        private Resident _resident;
        public Resident Resident
        {
            get { return _resident; }
            set { SetProperty(ref _resident, value, "Resident"); }
        }

        private Activity _activity;
        public Activity Activity
        {
            get { return _activity; }
            set { SetProperty(ref _activity, value, "Activity"); }
        }

        private string _comments;
        public string Comments
        {
            get { return _comments; }
            set { SetProperty(ref _comments, value, "Comments"); }
        }

        public static SensorEvent FromString(string curEventEntry, Dataset dataset)
        {
            SensorEvent sensorEvent = new SensorEvent();
            string[] tokenList = curEventEntry.Split(new Char[] { ',' });
            int numToken = tokenList.Count();
            if (numToken < 4)
                throw new ArgumentException("Number of Tokens in Sensor Event String is smaller than 4");
            // First Token: Date, Second Token: Time (with AM/PM), required
            sensorEvent.TimeTag = Convert.ToDateTime(tokenList[0] + " " + tokenList[1]);
            // Third Token: SensorID, required
            sensorEvent.Sensor = dataset.Site.GetSensor(tokenList[2], true);
            // Fourth Token: Status, required
            sensorEvent.SensorState = tokenList[3];
            // Fifth Token: Occupant
            if (numToken > 4 && !string.IsNullOrWhiteSpace(tokenList[3]))
                sensorEvent.Resident = dataset.GetResident(tokenList[4], true);
            else
                sensorEvent.Resident = Resident.NullResident;
            // Sixth Token: Activity Labels
            if (numToken > 5 && !string.IsNullOrWhiteSpace(tokenList[5]))
                sensorEvent.Activity = dataset.GetActivity(tokenList[5], true);
            else
                sensorEvent.Activity = Activity.NullActivity;
            // The Rest: Comments
            if (numToken > 6)
                sensorEvent.Comments = string.Join(",", tokenList.Skip(6));
            else
                sensorEvent.Comments = "";
            return sensorEvent;
        }

        public override string ToString()
        {
            return TimeTag.ToString("MM/dd/yyyy,HH:mm:ss,") + Sensor.Name + "," + SensorState + "," + Resident.Name + "," + Activity.Name + "," + Comments;
        }
    }
}

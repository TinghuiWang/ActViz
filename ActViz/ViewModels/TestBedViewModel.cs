using ActViz.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.ViewModels
{
    public class TestBedViewModel : ObservableObject
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        // tbname, description, active, created_on, timezone
        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        private bool _active;
        public bool Active
        {
            get { return _active; }
            set { SetProperty(ref _active, value); }
        }

        private DateTimeOffset _createdTime;
        public DateTimeOffset CreatedTime
        {
            get { return _createdTime; }
            set { SetProperty(ref _createdTime, value); }
        }

        private TimeZoneInfo _timeZone;
        public TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
            set { SetProperty(ref _timeZone, value); }
        }
    }

}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ActViz.ViewModels
{
    public class EventViewFilter
    {
        public HashSet<string> Activities { get; set; }
        public HashSet<string> SensorStatus { get; set; }
        public HashSet<string> Residents { get; set; }
        public HashSet<string> SensorCategories { get; set; }
        public bool HideEventsWithoutActivity { get; set; }
        public bool HideEventsWithoutResident { get; set; }

        [DefaultValue(true)]
        public bool IsActivityFilterEnabled { get; set; }

        public EventViewFilter()
        {
            Activities = new HashSet<string>();
            SensorStatus = new HashSet<string>();
            Residents = new HashSet<string>();
            SensorCategories = new HashSet<string>();
            HideEventsWithoutActivity = false;
            HideEventsWithoutResident = false;
        }
    }
}

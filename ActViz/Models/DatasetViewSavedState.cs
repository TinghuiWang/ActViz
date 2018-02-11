using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.Models
{
    public class DatasetViewSavedState
    {
        public DateTimeOffset Day { get; set; }
        public int EventInView { get; set; }
        public EventViewFilter Filter { get; set; }
    }
}

using ActViz.Helpers;
using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.ViewModels
{
    public class SensorViewModel : ObservableObject<Sensor>
    {
        public SensorViewModel(Sensor sensor) : base(sensor)
        {
            _sensorTypes = new ObservableCollection<SensorType>();
            _sensorCategories = new HashSet<string>();
            foreach (string type in sensor.Types)
            {
                SensorType sensorType = SensorType.GetSensorType(type);
                if(sensorType == null)
                {
                    // Which means this sensor type is not in the code base
                    sensorType = SensorType.AddSensorType(type);
                }
                _sensorTypes.Add(sensorType);
                if(!_sensorCategories.Contains(sensorType.Category))
                {
                    _sensorCategories.Add(sensorType.Category);
                }
            }
        }

        public string Name
        {
            get { return This.Name; }
            set { SetProperty(This.Name, value, () => This.Name = value); }
        }

        public List<string> Types
        {
            get { return This.Types; }
            set { SetProperty(This.Types, value, () => This.Types = value); }
        }

        private ObservableCollection<SensorType> _sensorTypes;
        public ObservableCollection<SensorType> SensorTypes { get { return _sensorTypes; } }

        private HashSet<string> _sensorCategories;
        public HashSet<string> SensorCategories { get { return _sensorCategories; } }

        public List<string> Serials
        {
            get { return This.Serials; }
        }

        public string Tag
        {
            get { return This.Tag; }
            set { SetProperty(This.Tag, value, () => This.Tag = value); }
        }

        public string Description
        {
            get { return This.Description; }
            set { SetProperty(This.Description, value, () => This.Description = value); }
        }

    }
}

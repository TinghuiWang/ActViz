using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ActViz.Models
{
    public class SensorType
    {
        private static readonly Dictionary<string, Color> _sensorCategoryColorDict = new Dictionary<string, Color>()
        {
            {"Motion", Colors.Red },
            {"Door", Colors.Green },
            {"Temperature", Colors.DarkGoldenrod },
            {"Item", Colors.BlueViolet },
            {"Light", Colors.Orange },
            {"LightSwitch", Colors.DarkOrange },
            {"Radio", Colors.LightPink },
            {"Battery", Colors.LightSeaGreen },
            {"Other", Colors.LightGray }
        };

        private string _name;
        public string Name { get { return _name; } set { _name = value; } }

        private string _category;
        public string Category { get { return _category; } set { _category = value; } }

        private string _strColor;
        public string StrColor { get { return _strColor; } set { _strColor = value; } }

        private Color _color;
        public Color Color { get { return _color; } set { _color = value; } }

        private static Dictionary<string, SensorType> _dictSensorTypeLookup = null;
        private static ObservableCollection<SensorType> _allSensorTypes = null;
        public static ObservableCollection<SensorType> AllSensorTypes
        {
            get
            {
                if (_allSensorTypes == null) InitAllSensorTypes();
                return _allSensorTypes;
            }
        }

        private static ObservableCollection<SensorCategory> _sensorCategoryCollection = null;
        public static ObservableCollection<SensorCategory> SensorCategoryCollection
        {
            get
            {
                if (_sensorCategoryCollection == null) InitAllSensorTypes();
                return _sensorCategoryCollection;
            }
        }

        public SensorType(string name, string category, Color color)
        {
            _name = name;
            _category = category;
            _color = color;
            _strColor = color.ToString();
        }

        private static void InitAllSensorTypes()
        {
            if (_allSensorTypes == null)
            {
                _allSensorTypes = new ObservableCollection<SensorType>() {
                    new SensorType("Kinect", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Thermostat-Setpoint", "Temperature", _sensorCategoryColorDict["Temperature"]),
                    new SensorType("RemCAS-Logic", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Weather-WindGust", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-BatteryVoltage", "Battery", _sensorCategoryColorDict["Battery"]),
                    new SensorType("ExperimenterSwitch-01", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Weather-Visibility", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("light_v2_30", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-MotionArea", "Motion", _sensorCategoryColorDict["Motion"]),
                    new SensorType("by", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Android_Rotation_Vector", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Weather-WindDirection", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Thermostat-Temperature", "Temperature", _sensorCategoryColorDict["Temperature"]),
                    new SensorType("Weather-Condition", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("contactsingle_glassbreakdetector_c4_32", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-Radio_error", "Radio", _sensorCategoryColorDict["Radio"]),
                    new SensorType("MotionArea-01", "Motion", _sensorCategoryColorDict["Motion"]),
                    new SensorType("RemCAS-v01", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Temperature", "Temperature", _sensorCategoryColorDict["Temperature"]),
                    new SensorType("Weather-UV", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Office", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("test", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Accel-Gyro", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("ShimmerTilt-01", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("WebButton", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-LightLevel", "Light", _sensorCategoryColorDict["Light"]),
                    new SensorType("Zigbee-NetSecCounter", "Radio", _sensorCategoryColorDict["Radio"]),
                    new SensorType("PromptButton", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("EventGenerator", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("OneMeter-kWh", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("rPi-Relay", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Weather-Temperature", "Temperature", _sensorCategoryColorDict["Temperature"]),
                    new SensorType("Android_Gyroscope", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Zigbee-Channel", "Radio", _sensorCategoryColorDict["Radio"]),
                    new SensorType("Control4-BatteryPercent", "Battery", _sensorCategoryColorDict["Battery"]),
                    new SensorType("Control4-Seat", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Weather-Humidity", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Thermostat-Link", "Temperature", _sensorCategoryColorDict["Temperature"]),
                    new SensorType("Inertial_Sensors", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("OneMeter-watts", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Asterisk_Agent", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-Temperature", "Temperature", _sensorCategoryColorDict["Temperature"]),
                    new SensorType("RemCAS-UI", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-Relay", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("ExperimenterSwitch-02", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Weather-Event", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("AD-01", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("KVA", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("RAT", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Door-01", "Door", _sensorCategoryColorDict["Door"]),
                    new SensorType("ReminderFrontend", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("BMP085_Temperature", "Temperature", _sensorCategoryColorDict["Temperature"]),
                    new SensorType("Control4-Door", "Door", _sensorCategoryColorDict["Door"]),
                    new SensorType("Reminder", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("K-30_CO2_Sensor", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("BMP085_Pressure", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-Radio", "Radio", _sensorCategoryColorDict["Radio"]),
                    new SensorType("Power", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("C4-TimeZone", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("cardaccess_wirelesscontact_26", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Asterisk", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("GridEye", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("PromptAudio", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-Motion", "Motion", _sensorCategoryColorDict["Motion"]),
                    new SensorType("Prediction", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Android_Magnetic_Field", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("contactsingle_doorcontactsensor_c4_31", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-Light", "Light", _sensorCategoryColorDict["Light"]),
                    new SensorType("Motion-01", "Motion", _sensorCategoryColorDict["Motion"]),
                    new SensorType("Weather-Wind", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("button", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Zigbee-MacAddr", "Radio", _sensorCategoryColorDict["Radio"]),
                    new SensorType("Control4-TV", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("unknown", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Zigbee-Structure", "Radio", _sensorCategoryColorDict["Radio"]),
                    new SensorType("Control4-Button", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("control", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Item-01", "Item", _sensorCategoryColorDict["Item"]),
                    new SensorType("Weather-Precipitation", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Insteon", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("GeneratedEvent", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Thermostat-Heater", "Temperature", _sensorCategoryColorDict["Temperature"]),
                    new SensorType("ShimmerSixAxis-01", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-Item", "Item", _sensorCategoryColorDict["Item"]),
                    new SensorType("Weather-Pressure", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Control4-LightSensor", "Light", _sensorCategoryColorDict["Light"]),
                    new SensorType("Android_Accelerometer", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Thermostat-Window", "Temperature", _sensorCategoryColorDict["Temperature"]),
                    new SensorType("contactsingle_motionsensor_23", "Motion", _sensorCategoryColorDict["Motion"]),
                    new SensorType("system", "Other", _sensorCategoryColorDict["Other"]),
                    new SensorType("Insteon-Relay", "Other", _sensorCategoryColorDict["Other"]),
                    GeneralMotion,
                    GeneralDoor,
                    GeneralItem,
                    GeneralTemperature,
                    GeneralLight,
                    GeneralPower,
                    GeneralOther
                };
                // Initialize Sensor Type Lookup Dictionary
                _dictSensorTypeLookup = new Dictionary<string, SensorType>();
                foreach (SensorType sensorType in _allSensorTypes)
                {
                    _dictSensorTypeLookup.Add(sensorType.Name, sensorType);
                }
                // Initialize Sensor Category Collection for UI Filters
                _sensorCategoryCollection = new ObservableCollection<SensorCategory>();
                foreach (string category in _sensorCategoryColorDict.Keys)
                {
                    _sensorCategoryCollection.Add(new SensorCategory() { Name = category, Color = _sensorCategoryColorDict[category] });
                }
            }
        }

        private static readonly SensorType _generalMotion = new SensorType("GeneralMotion", "Motion", Colors.Red);
        public static SensorType GeneralMotion { get { return _generalMotion; } }

        private static readonly SensorType _generalDoor = new SensorType("GeneralDoor", "Door", Colors.Green);
        public static SensorType GeneralDoor { get { return _generalDoor; } }

        private static readonly SensorType _generalItem = new SensorType("GeneralItem", "Item", Colors.BlueViolet);
        public static SensorType GeneralItem { get { return _generalItem; } }

        private static readonly SensorType _generalTemperature = new SensorType("GeneralTemperature", "Temperature", Colors.DarkGoldenrod);
        public static SensorType GeneralTemperature { get { return _generalTemperature; } }

        private static readonly SensorType _generalLight = new SensorType("GeneralLight", "Light", Colors.Orange);
        public static SensorType GeneralLight { get { return _generalLight; } }

        private static readonly SensorType _generalPower = new SensorType("GeneralPower", "Other", Colors.White);
        public static SensorType GeneralPower { get { return _generalPower; } }

        private static readonly SensorType _generalOther = new SensorType("Other", "Other", Colors.White);
        public static SensorType GeneralOther { get { return _generalOther; } }

        internal static SensorType AddSensorType(string type)
        {
            SensorType newType = new SensorType(type, "Other", _sensorCategoryColorDict["Other"]);
            _allSensorTypes.Add(newType);
            _dictSensorTypeLookup.Add(newType.Name, newType);
            return newType;
        }

        public static SensorType GuessSensorTypeFromName(string sensorName)
        {
            switch (sensorName[0])
            {
                case 'M':
                    return _generalMotion;
                case 'D':
                    return _generalDoor;
                case 'T':
                    return _generalTemperature;
                case 'I':
                    return _generalItem;
                case 'L':
                    return _generalLight;
                case 'P':
                    return _generalPower;
                default:
                    return _generalOther;
            }
        }

        public static Color GetBestColorForSensor(SensorViewModel sensorViewModel, string priorityCategory = null)
        {
            if (priorityCategory != null && sensorViewModel.SensorCategories.Contains(priorityCategory))
            {
                return _sensorCategoryColorDict[priorityCategory];
            }

            // Motion, Item, Door, Light, Temperature, Power
            List<string> categoryDefaultPriority = new List<string> { "Motion", "Item", "Door", "Temperature", "Light", "LightSwitch", "Battery", "Radio", "Other" };
            foreach(string category in categoryDefaultPriority)
            {
                if(sensorViewModel.SensorCategories.Contains(category))
                {
                    return _sensorCategoryColorDict[category];
                }
            }
            return _sensorCategoryColorDict["Other"];
        }

        public static Color GetColorFromSensorType(string type)
        {
            SensorType sensorType = GetSensorType(type);
            if (sensorType != null)
            {
                return sensorType.Color;
            }
            return Colors.White;
        }

        public static int GetSensorTypeIndex(string type)
        {
            int i;
            if (_allSensorTypes == null)
            {
                InitAllSensorTypes();
            }
            SensorType sensorType;
            if (_dictSensorTypeLookup.TryGetValue(type, out sensorType))
            {
                return _allSensorTypes.IndexOf(sensorType);
            }
            return -1;
        }

        public static SensorType GetSensorType(string type)
        {
            if (_allSensorTypes == null)
            {
                InitAllSensorTypes();
            }
            if (_dictSensorTypeLookup.TryGetValue(type, out SensorType sensorType))
            {
                return sensorType;
            }
            return null;
        }

        public static SensorCategory GetSensorCategory(string category)
        {
            return _sensorCategoryCollection.FirstOrDefault(x => x.Name == category);
        }
    }

    public class SensorCategory
    {
        public string Name { get; set; }
        public Color Color { get; set; }
    }
}

using ActViz.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ActViz.Models
{
    public class Site
    {
        [JsonProperty("floorplan")]
        public string Floorplan { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sensors")]
        public List<Sensor> Sensors { get; set; }

        [JsonProperty("timezone")]
        public string TimeZoneString { get; set; }

        [JsonIgnore]
        public TimeZoneInfo TimeZone { get; set; }

        [JsonIgnore]
        public StorageFolder Folder { get; set; }

        [JsonIgnore]
        public ImageSource ImgFloorPlan { get; set; }

        //[JsonIgnore]
        //public byte[] ByteFloorPlan { get; set; }

        //[JsonIgnore]
        //public int FloorPlanHeight { get; set; }

        //[JsonIgnore]
        //public int FloorPlanWidth { get; set; }

        [JsonIgnore]
        public MemoryStream FloorPlanStream { get; set; }

        [JsonIgnore]
        private Dictionary<string, int> _sensorDictionary = new Dictionary<string, int>();

        [JsonIgnore]
        private bool _isModified = false;

        [JsonIgnore]
        public bool IsModified
        {
            get { return _isModified; }
            set { _isModified = value; }
        }

        public async Task WriteToFolderAsync()
        {
            StorageFile siteMetaFile = await Folder.CreateFileAsync("site.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(siteMetaFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static async Task<Site> LoadFromFolderAsync(StorageFolder folder)
        {
            StorageFile siteMetaFile = await folder.GetFileAsync("site.json");
            string siteMetaString = await FileIO.ReadTextAsync(siteMetaFile);
            Site site = JsonConvert.DeserializeObject<Site>(siteMetaString);
            if (site.TimeZoneString is null)
            {
                site.TimeZoneString = "America/Los_Angeles";
            }
            site.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConverter.TZConvert.IanaToWindows(site.TimeZoneString));
            site.Folder = folder;
            StorageFile floorplanFile = await folder.GetFileAsync(site.Floorplan);
            var stream = await floorplanFile.OpenReadAsync();
            //site.FloorPlanStream = new MemoryStream();
            //await stream.AsStreamForRead().CopyToAsync(site.FloorPlanStream);
            //IRandomAccessStream floorplanStream = await floorplanFile.OpenAsync(FileAccessMode.Read);
            // Read bitmap into software bitmap
            //BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(floorplanStream);
            //PixelDataProvider pixelDataProvider = await bitmapDecoder.GetPixelDataAsync(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Premultiplied, null, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
            //site.ByteFloorPlan = pixelDataProvider.DetachPixelData();
            //site.FloorPlanHeight = (int) bitmapDecoder.PixelHeight;
            //site.FloorPlanWidth = (int) bitmapDecoder.PixelWidth;
            // Setup Floorplan Imagesource
            BitmapImage bitmapFloorplan = new BitmapImage();
            bitmapFloorplan.SetSource(stream);
            site.ImgFloorPlan = bitmapFloorplan;
            site.PrepareData();
            return site;
        }

        public void PrepareData()
        {
            for (int i = 0; i < Sensors.Count; i++)
            {
                _sensorDictionary.Add(Sensors[i].Name, i);
            }
        }

        public bool AddSensor(Sensor sensor)
        {
            if (_sensorDictionary.TryGetValue(sensor.Name, out int i))
            {
                return false;
            }
            else
            {
                Sensors.Add(sensor);
                _sensorDictionary.Add(sensor.Name, Sensors.Count - 1);
                return true;
            }
        }

        public Sensor GetSensor(string name)
        {
            if (_sensorDictionary.TryGetValue(name, out int i))
            {
                return Sensors[i];
            }
            else
            {
                return null;
            }
        }

        public Sensor GetSensor(string name, bool createIfNotExist)
        {
            Logger appLog = Logger.Instance;
            Sensor sensor = GetSensor(name);
            if (sensor == null && createIfNotExist)
            {
                appLog.Warn(this.GetType().ToString(), string.Format("Sensor {0} not Found. Add to site {1}.", name, Name));
                sensor = new Sensor
                {
                    Name = name,
                    Serials = new List<string>(),
                    LocX = 0,
                    LocY = 0,
                    SizeX = 0.03,
                    SizeY = 0.03
                };
                if (AddSensor(sensor))
                {
                    return sensor;
                }
                else
                {
                    return null;
                }
            }
            return sensor;
        }
    }
}

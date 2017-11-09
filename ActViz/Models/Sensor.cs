using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.Models
{
    public class Sensor
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; }

        [JsonProperty("locX")]
        public double LocX { get; set; }

        [JsonProperty("locY")]
        public double LocY { get; set; }

        [JsonProperty("sizeX")]
        public double SizeX { get; set; }

        [JsonProperty("sizeY")]
        public double SizeY { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("serial")]
        public List<string> Serial { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonIgnore]
        public double FontSize { get; set; }
    }
}

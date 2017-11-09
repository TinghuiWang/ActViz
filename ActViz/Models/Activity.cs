using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.Models
{
    public class Activity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("is_noise")]
        public bool IsNoise { get; set; }

        [JsonProperty("is_ignored")]
        public bool IsIgnored { get; set; }

        public static Activity NullActivity = new Activity
        {
            Name = "",
            Color = "#FFC0C0C0",
            IsNoise = false,
            IsIgnored = false
        };
    }
}

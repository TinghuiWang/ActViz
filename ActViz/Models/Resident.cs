using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.Models
{
    public class Resident
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        static public Resident NullResident = new Resident
        {
            Name = "",
            Color = "#FFC0C0C0"
        };
    }
}

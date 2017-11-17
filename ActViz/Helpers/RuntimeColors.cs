using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ActViz.Helpers
{
    public class RuntimeColors
    {
        static readonly RuntimeColors _instance = new RuntimeColors();

        public static RuntimeColors Instance
        {
            get { return _instance; }
        }

        private Dictionary<Color, string> _colorDictionary = new Dictionary<Color, string>();
        public Dictionary<Color, string> ColorDictionary { get { return _colorDictionary; } }

        private ObservableCollection<Color> _colorCollection = new ObservableCollection<Color>();
        public ObservableCollection<Color> ColorCollection { get { return _colorCollection; } }

        private RuntimeColors()
        {
            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                _colorCollection.Add((Color)color.GetValue(null));
                try
                {
                    _colorDictionary.Add((Color)color.GetValue(null), color.Name);
                }
                catch (Exception) { }
            }
        }

        public static string ConvertToString(Color color)
        {
            return color.ToString();
        }

        public static Color ConvertFromHexString(string colorHex)
        {
            if (colorHex != null && colorHex.Length == 9)
            {
                byte r = byte.Parse(colorHex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(colorHex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(colorHex.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(255, r, g, b);
            }
            else
                return Colors.Black;
        }
    }
}

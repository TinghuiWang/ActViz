using ActViz.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;

namespace ActViz.Converters
{
    public class ColorToNameStringConverter : IValueConverter
    {
        Dictionary<Color, string> colorDictionary = RuntimeColors.Instance.ColorDictionary;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string colorName;
            if (colorDictionary.TryGetValue((Color)value, out colorName))
            {
                colorName = colorName + " (" + ((Color)value).ToString() + ")";
                return colorName;
            }
            else
            {
                return "(" + ((Color)value).ToString() + ")";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

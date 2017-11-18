using ActViz.Helpers;
using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ActViz.ViewModels
{
    public class ActivityViewModel : ObservableObject<Activity>
    {
        public string Name
        {
            get { return This.Name; }
            set { SetProperty(This.Name, value, () => This.Name = value); }
        }

        private Color _color;
        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    SetProperty(ref _color, value);
                    ColorString = RuntimeColors.ConvertToString(_color);
                }
            }
        }

        public string ColorString
        {
            get { return This.Color; }
            set
            {
                if (ColorString != value)
                {
                    SetProperty(This.Color, value, () => This.Color = value);
                    Color = RuntimeColors.ConvertFromHexString(This.Color);
                }
            }
        }

        public bool IsNoise
        {
            get { return This.IsNoise; }
            set { SetProperty(This.IsNoise, value, () => This.IsNoise = value); }
        }

        public bool IsIgnored
        {
            get { return This.IsIgnored; }
            set { SetProperty(This.IsIgnored, value, () => This.IsIgnored = value); }
        }

        public ActivityViewModel(Activity activity) : base (activity)
        {
            Color = RuntimeColors.ConvertFromHexString(activity.Color);
        }

        private static ActivityViewModel _nullActivity = new ActivityViewModel(Activity.NullActivity);
        public static ActivityViewModel NullActivity { get { return _nullActivity; } }
    }
}

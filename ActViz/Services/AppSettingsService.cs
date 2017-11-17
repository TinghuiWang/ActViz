using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ActViz.Services
{
    public static class AppSettingsService
    {
        public static T RetrieveFromSettings<T>(string key, T defaultValue, ApplicationDataContainer localSettings = null)
        {
            if (localSettings == null) localSettings = ApplicationData.Current.LocalSettings;
            try
            {
                return (T)localSettings.Values[key];
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;

namespace ActViz.Helpers
{
    public class Logger : ObservableObject
    {
        public const int LOGGING_DEBUG = 0;
        public const int LOGGING_INFO = 1;
        public const int LOGGING_WARN = 2;
        public const int LOGGING_ERROR = 3;

        public int LogLevel { get; set; }
        private CoreDispatcher dispatcher;

        static readonly Logger _instance = new Logger();

        public static Logger Instance
        {
            get { return _instance; }
        }

        private string _log;
        public string Log { get { return _log; } }
        public StorageFile storageFile;

        private Logger()
        {
            _log = null;
            LogLevel = LOGGING_INFO;
            dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
        }

        /// <summary>
        /// Thread and task safe log update - however, as it uses dispatcher to update the UI thread, UI thread should never block.
        /// </summary>
        /// <param name="text"></param>
        private async void AppendLogAsync(string text)
        {
            // Check if logfile is opened
            //if (storageFile == null)
            //{
            //    // Open log file from local storage. Create if it does not exist.
            //    StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            //    storageFile = await storageFolder.CreateFileAsync("app.log", CreationCollisionOption.OpenIfExists);
            //}
            // Append to Log file
            //await FileIO.AppendTextAsync(storageFile, text);
            // Update Log on screen
            if (dispatcher == null)
            {
                dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
            }
            if (dispatcher != null)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SetProperty(ref _log, _log + text, "Log");
                });
            }
            else
            {
                _log = _log + text;
            }
        }

        public void Warn(string sender, string message)
        {
            if (LogLevel <= LOGGING_WARN)
            {
                if (_log == null) _log = "";
                string new_log = string.Format("[{0}] Warn: {1} : {2}\n", DateTime.Now.ToString(), sender, message);
                AppendLogAsync(new_log);
            }
        }

        public void Info(string sender, string message)
        {
            if (LogLevel <= LOGGING_INFO)
            {
                if (_log == null) _log = "";
                string new_log = string.Format("[{0}] Info: {1} : {2}\n", DateTime.Now.ToString(), sender, message);
                AppendLogAsync(new_log);
            }
        }

        public void Error(string sender, string message)
        {
            if (LogLevel <= LOGGING_ERROR)
            {
                if (_log == null) _log = "";
                string new_log = string.Format("[{0}] Error: {1} : {2}\n", DateTime.Now.ToString(), sender, message);
                AppendLogAsync(new_log);
            }
        }

        public void Debug(string sender, string message)
        {
            if (LogLevel <= LOGGING_DEBUG)
            {
                if (_log == null) _log = "";
                string new_log = string.Format("[{0}] Debug: {1} : {2}\n", DateTime.Now.ToString(), sender, message);
                AppendLogAsync(new_log);
            }
        }
    }

}

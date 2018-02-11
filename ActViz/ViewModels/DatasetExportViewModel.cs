using ActViz.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.ViewModels
{
    public class DatasetExportViewModel : ObservableObject
    {
        private bool _exportInCSV = true;
        public bool ExportInCSV
        {
            get { return _exportInCSV; }
            set { SetProperty(ref _exportInCSV, value); }
        }

        private bool _exportInTxt = false;
        public bool ExportInTxt
        {
            get { return _exportInTxt; }
            set { SetProperty(ref _exportInTxt, value); }
        }

        private string _datasetName = "";
        public string DatasetName
        {
            get { return _datasetName; }
            set { SetProperty(ref _datasetName, value); }
        }

        private DateTimeOffset _exportStartDate;
        public DateTimeOffset ExportStartDate
        {
            get { return _exportStartDate; }
            set { SetProperty(ref _exportStartDate, value); }
        }

        private DateTimeOffset _exportStopDate;
        public DateTimeOffset ExportStopDate
        {
            get { return _exportStopDate; }
            set { SetProperty(ref _exportStopDate, value); }
        }

        private bool _datasetRenameEnabled = false;
        public bool DatasetRenameEnabled
        {
            get { return _datasetRenameEnabled; }
            set { SetProperty(ref _datasetRenameEnabled, value); }
        }

        private bool _exportDateSelectionEnabled = false;
        public bool ExportDateSelectionEnabled
        {
            get { return _exportDateSelectionEnabled; }
            set { SetProperty(ref _exportDateSelectionEnabled, value); }
        }

        public DatasetEventsViewModel DatasetEvents;

        public DatasetExportViewModel(DatasetEventsViewModel datasetEvents)
        {
            this.DatasetEvents = datasetEvents;
            this.ExportStartDate = datasetEvents.FirstEventDate;
            this.ExportStopDate = datasetEvents.LastEventDate;
            this.DatasetName = datasetEvents.Dataset.Name;
        }
    }
}

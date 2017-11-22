using ActViz.Helpers;
using ActViz.Models;
using ActViz.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ActViz.ViewModels
{
    public class DatasetSelectionViewModel : ObservableObject
    {
        public ObservableCollection<Dataset> DatasetList = new ObservableCollection<Dataset>();
        private Dictionary<string, int> _datasetDictionary = new Dictionary<string, int>();
        private List<Site> _siteList;

        private Dataset _datasetSelected;
        public Dataset DatasetSelected
        {
            get { return _datasetSelected; }
            set
            {
                SetProperty(ref _datasetSelected, value);
                IsDatasetSelected = (value != null);
            }
        }

        private bool _isDatasetSelected = false;
        public bool IsDatasetSelected
        {
            get { return _isDatasetSelected; }
            set { SetProperty(ref _isDatasetSelected, value); }
        }

        public async Task LoadFromLocalAsync()
        {
            _siteList = await LocalMetadataService.LoadSitesAsync();
            List<Dataset> datasetList = await LocalMetadataService.LoadDatasetsAsync(_siteList);
            DatasetList.Clear();
            _datasetDictionary.Clear();
            int i = 0;
            foreach(Dataset dataset in datasetList)
            {
                DatasetList.Add(dataset);
                _datasetDictionary.Add(dataset.Name, i);
                i++;
            }
        }

        internal int GetDatasetId(string name)
        {
            int localDatasetId;
            if (_datasetDictionary.TryGetValue(name, out localDatasetId))
            {
                return localDatasetId;
            }
            else
            {
                return -1;
            }
        }

        internal int GetDatasetId(Dataset dataset)
        {
            return GetDatasetId(dataset.Name);
        }

        internal async Task AddDatasetAsync(Dataset dataset)
        {
            // TODO: Check and prompt for overwrite
            // Copy to Local Data
            StorageFolder sitesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("datasets", CreationCollisionOption.OpenIfExists);
            StorageFolder targetDatasetFolder = await sitesFolder.CreateFolderAsync(dataset.Name, CreationCollisionOption.OpenIfExists);
            IReadOnlyList<StorageFile> fileList = await dataset.Folder.GetFilesAsync();
            for (int i = 0; i < fileList.Count; i++)
            {
                await fileList[i].CopyAsync(targetDatasetFolder);
            }
            // Reload all data
            await LoadFromLocalAsync();
        }

        internal async Task RemoveDatasetAsync(Dataset datasetSelected)
        {
            if (DatasetSelected == datasetSelected) DatasetSelected = null;
            DatasetList.Remove(datasetSelected);
            await datasetSelected.Folder.DeleteAsync();
        }
    }
}

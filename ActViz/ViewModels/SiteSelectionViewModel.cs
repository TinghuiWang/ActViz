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
    public class SiteSelectionViewModel : ObservableObject
    {
        public ObservableCollection<Site> SiteList = new ObservableCollection<Site>();
        private Dictionary<string, int> _siteDictionary = new Dictionary<string, int>();

        private Site _siteSelected;
        public Site SiteSelected
        {
            get { return _siteSelected; }
            set
            {
                SetProperty(ref _siteSelected, value);
                IsSiteSelected = (value != null);
            }
        }

        private bool _isSiteSelected = false;
        public bool IsSiteSelected
        {
            get { return _isSiteSelected; }
            set { SetProperty(ref _isSiteSelected, value); }
        }

        public async Task LoadFromLocalAsync()
        {
            var siteList = await LocalMetadataService.LoadSitesAsync();
            int i = 0;
            foreach (Site site in siteList)
            {
                SiteList.Add(site);
                _siteDictionary.Add(site.Name, i);
                i++;
            }
        }

        internal int GetSiteId(string name)
        {
            if (_siteDictionary.TryGetValue(name, out int localSiteId))
            {
                return localSiteId;
            }
            else
            {
                return -1;
            }
        }

        internal int GetSiteId(Site site)
        {
            return GetSiteId(site.Name);
        }

        internal async Task AddSiteAsync(Site site)
        {
            // TODO: Check and prompt for overwrite
            // Copy to Local Data
            StorageFolder sitesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("sites", CreationCollisionOption.OpenIfExists);
            StorageFolder targetSiteFolder = await sitesFolder.CreateFolderAsync(site.Name, CreationCollisionOption.OpenIfExists);
            IReadOnlyList<StorageFile> fileList = await site.Folder.GetFilesAsync();
            for (int i = 0; i < fileList.Count; i++)
            {
                await fileList[i].CopyAsync(targetSiteFolder);
            }
            // Reload all sites
            await LoadFromLocalAsync();
        }
    }
}

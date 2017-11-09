using ActViz.Helpers;
using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ActViz.Services
{
    public static class LocalMetadataService
    {
        public static async Task<List<Dataset>> LoadDatasetsAsync(List<Site> siteList)
        {
            List<Dataset> datasetList = new List<Dataset>();
            // Load Sites
            StorageFolder datasetsFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("datasets", CreationCollisionOption.OpenIfExists);
            IReadOnlyList<StorageFolder> datasetFolderList = await datasetsFolder.GetFoldersAsync();
            for (int i = 0; i < datasetFolderList.Count; i++)
            {
                // Open meta-data file
                Dataset dataset = await Dataset.LoadMetadataFromFolderAsync(datasetFolderList[i]);
                var site = siteList.FirstOrDefault(x => x.Name == dataset.SiteName);
                if (site != null)
                {
                    dataset.Site = site;
                }
                else
                {
                    Logger.Instance.Warn("LocalMetadataService", string.Format("Site {0} for dataset {1} not found.", dataset.SiteName, dataset.Name));
                }
                datasetList.Add(dataset);
            }
            return datasetList;
        }

        public static async Task<List<Site>> LoadSitesAsync()
        {
            List<Site> siteList = new List<Site>();
            // Load Sites
            StorageFolder sitesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("sites", CreationCollisionOption.OpenIfExists);
            IReadOnlyList<StorageFolder> sitesFolderList = await sitesFolder.GetFoldersAsync();
            for (int i = 0; i < sitesFolderList.Count; i++)
            {
                // Open meta-data file
                Site site = await Site.LoadFromFolderAsync(sitesFolderList[i]);
                siteList.Add(site);
            }
            return siteList;
        }
    }
}

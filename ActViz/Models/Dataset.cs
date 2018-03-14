using ActViz.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ActViz.Models
{
    /// <summary>
    /// Dataset class corresponds a smarthome dataset.
    /// It includes information about the location of the dataset, the smart home site
    /// this dataset is recorded, the number of residents and where to get the recorded
    /// sensor data (with or without annotation).
    /// </summary>
    public class Dataset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("activities")]
        public List<Activity> Activities { get; set; }

        [JsonProperty("residents")]
        public List<Resident> Residents { get; set; }

        [JsonProperty("site")]
        public string SiteName { get; set; }

        [JsonIgnore]
        public Site Site { get; set; }

        [JsonProperty("vhost")]
        public string VirtualHost { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonIgnore]
        public StorageFolder Folder { get; set; }

        [JsonIgnore]
        private Dictionary<string, int> _activityDictionary = new Dictionary<string, int>();

        [JsonIgnore]
        private Dictionary<string, int> _residentDictionary = new Dictionary<string, int>();

        [JsonIgnore]
        private bool _isModified = false;

        [JsonIgnore]
        public bool IsModified { get { return _isModified; } set { _isModified = value; } }

        public async Task WriteMetadataToFolderAsync()
        {
            Activities.Sort(delegate (Activity x, Activity y)
            {
                if (x.Name == null && y.Name == null) return 0;
                else if (x.Name == null) return -1;
                else if (y.Name == null) return 1;
                else return x.Name.CompareTo(y.Name);
            });
            Residents.Sort(delegate (Resident x, Resident y)
            {
                if (x.Name == null && y.Name == null) return 0;
                else if (x.Name == null) return -1;
                else if (y.Name == null) return 1;
                else return x.Name.CompareTo(y.Name);
            });
            StorageFile datasetMetaFile = await Folder.CreateFileAsync("dataset.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(datasetMetaFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static void CheckDataset(Dataset dataset)
        {
            // Make sure that the dataset loaded does not have null fields, duplicated activity labels or resident names
            if (dataset.Name == null)
            {
                dataset.Name = "Unknown Dataset";
            }
            Dictionary<string, int> duplicateDict = new Dictionary<string, int>();
            foreach (Activity activity in dataset.Activities)
            {
                if (activity.Name == null)
                {
                    activity.Name = "_";
                    dataset.IsModified = true;
                }
                if (duplicateDict.ContainsKey(activity.Name))
                {
                    duplicateDict[activity.Name]++;
                    activity.Name += string.Format("_({0})", duplicateDict[activity.Name]);
                    Logger.Instance.Warn("LoadDatasetsAsync", string.Format(
                        "Duplicate Activity {0} found in dataset {1}", activity.Name, dataset.Name
                        ));
                    dataset.IsModified = true;
                }
                else
                {
                    duplicateDict.Add(activity.Name, 0);
                }
            }
            duplicateDict.Clear();
            foreach (Resident resident in dataset.Residents)
            {
                if (resident.Name == null)
                {
                    resident.Name = "_";
                    dataset.IsModified = true;
                }
                if (duplicateDict.ContainsKey(resident.Name))
                {
                    duplicateDict[resident.Name]++;
                    resident.Name += string.Format("_({0})", duplicateDict[resident.Name]);
                    Logger.Instance.Warn("LoadDatasetsAsync", string.Format(
                        "Duplicate Resident {0} found in dataset {1}", resident.Name, dataset.Name
                        ));
                    dataset.IsModified = true;
                }
                else
                {
                    duplicateDict.Add(resident.Name, 0);
                }
            }
        }

        public static async Task<Dataset> LoadMetadataFromFolderAsync(StorageFolder folder)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            StorageFile datasetMetaFile = await folder.GetFileAsync("dataset.json");
            string datasetMetaString = await FileIO.ReadTextAsync(datasetMetaFile);
            Dataset dataset = JsonConvert.DeserializeObject<Dataset>(datasetMetaString, jsonSerializerSettings);
            CheckDataset(dataset);
            dataset.Folder = folder;
            dataset.PrepareData();
            return dataset;
        }

        public void PrepareData()
        {
            for (int i = 0; i < Residents.Count; i++)
            {
                _residentDictionary.Add(Residents[i].Name, i);
            }
            for (int i = 0; i < Activities.Count; i++)
            {
                _activityDictionary.Add(Activities[i].Name, i);
            }
        }

        public Activity GetActivity(string name)
        {
            if (name == "")
                return Activity.NullActivity;
            if (_activityDictionary.TryGetValue(name, out int i))
            {
                return Activities[i];
            }
            else
            {
                return null;
            }
        }

        public Activity GetActivity(string name, bool createIfNotExist)
        {
            Logger appLog = Logger.Instance;
            Activity activity = GetActivity(name);
            if (activity == null && createIfNotExist)
            {
                appLog.Warn(this.GetType().ToString(), string.Format("Activity {0} not Found. Add to dataset {1}.", name, Name));
                activity = new Activity
                {
                    Name = name,
                    IsNoise = false,
                    IsIgnored = false,
                    Color = "#FFFFFF"
                };
                if (AddActivity(activity))
                {
                    return activity;
                }
                else
                {
                    return null;
                }
            }
            return activity;
        }

        public Resident GetResident(string name)
        {
            if (name == "")
            {
                return Resident.NullResident;
            }

            if (_residentDictionary.TryGetValue(name, out int i))
            {
                return Residents[i];
            }
            else
            {
                return null;
            }
        }

        public Resident GetResident(string name, bool createIfNotExist = false)
        {
            Logger appLog = Logger.Instance;
            Resident resident = GetResident(name);
            if (resident == null && createIfNotExist)
            {
                appLog.Warn(this.GetType().ToString(), string.Format("Resident {0} not Found. Add to dataset {1}.", name, Name));
                resident = new Resident
                {
                    Name = name,
                    Color = "#FFFFFFFF"
                };
                if (AddResident(resident))
                {
                    return resident;
                }
                else
                {
                    return null;
                }
            }
            return resident;
        }

        /// <summary>
        /// Add Resident to resident list. Resident search dictionary is updated as well.
        /// If resident of same name exists, the add operation will fail.
        /// If add operation succeed, return True. Return False otherwise.
        /// </summary>
        /// <param name="resident"></param>
        /// <returns></returns>
        public bool AddResident(Resident resident)
        {
            if (_residentDictionary.TryGetValue(resident.Name, out int i))
            {
                return false;
            }
            else
            {
                Residents.Add(resident);
                _residentDictionary.Add(resident.Name, Residents.Count - 1);
                _isModified = true;
                return true;
            }
        }

        /// <summary>
        /// Add Activity to activity list. Activity search dictionary is updated as well.
        /// If an activity of the same name exists, the add operation will fail.
        /// Return true when the operation succeed. Reture false otherwise.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public bool AddActivity(Activity activity)
        {
            if (_activityDictionary.TryGetValue(activity.Name, out int i))
            {
                return false;
            }
            else
            {
                Activities.Add(activity);
                _activityDictionary.Add(activity.Name, Activities.Count - 1);
                _isModified = true;
                return true;
            }
        }
    }

}

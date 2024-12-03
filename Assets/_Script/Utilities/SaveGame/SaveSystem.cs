using System.Collections.Generic;
using System.Linq;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Utilities.SaveGame
{
    public class SaveSystem : PersistentSingleton<SaveSystem>
    {
        
        [SerializeField] private bool _debug = false;
        
        public bool LoadData<T>(string SaveName) where T : ISaveGame
        {
            if (IsNewSave<T>(SaveName))
            {
                //Debug.Log($"It is a new save {SaveName}_{typeof(T).Name}");
                return false;
            }
            else
            {
                //Debug.Log($"load Saved {SaveName}_{typeof(T).Name}");
                Load<T>(SaveName);
                return true;
            }
        }
        
        private string GetInternalName<T>(string SaveName)
        {
            return $"{SaveName}_{typeof(T).Name}";
        }

        private bool IsNewSave<T>(string SaveName)
        {
            return !ES3.KeyExists(GetInternalName<T>(SaveName));
        }

        private void LoadDefaultData<T>() where T : ISaveGame
        {
            Debug.Log("target " + (typeof(T)) + "has no initial data and was able to load, hence load default data from scriptableObject");
            List<T> saveServices = GetSaveGameServices<T>();
            foreach (var o in saveServices)
            {
                o.LoadDefaultData();
            }
        }

        public void SaveData<T>(string SaveName) where T : ISaveGame
        {
            //Create Index for Each Type of Data
            List<T> saveServices = GetSaveGameServices<T>();
            List<string> fileIndices = new List<string>();
        
            
            foreach (var service in saveServices)
            {
                string key = GenerateUniqueKeyForService(service); // Generate unique keys
                object data = service.OnSaveData();
                ES3.Save(key, data); // Save the data
                fileIndices.Add(key);
            }
            if(_debug)
                Debug.Log($"Saving... {SaveName}_{typeof(T).Name}");
            ES3.Save(GetInternalName<T>(SaveName), fileIndices); // Save the index of all keys
        }
    
        private void Load<T>(string SaveName) where T : ISaveGame
        {   
            string typeSpecificSaveName = $"{SaveName}_{typeof(T).Name}";
        
            if (!ES3.KeyExists(typeSpecificSaveName))
            {
                Debug.LogWarning($"No saved data found for {typeSpecificSaveName}");
                return;
            }

            List<string> fileIndices = ES3.Load<List<string>>(typeSpecificSaveName);
            List<T> saveServices = GetSaveGameServices<T>();

            foreach (string key in fileIndices)
            {
                if (ES3.KeyExists(key)) // Consider specifying the file if not using the default
                {
                    object data = ES3.Load<object>(key); // Consider specifying a more specific type if possible
                    var service = saveServices.FirstOrDefault(s => GenerateUniqueKeyForService(s) == key);
                    if (service != null)
                    {
                        service.OnLoadData(data);
                    }
                }
            }
        }
    
        private List<T> GetSaveGameServices<T>() where T : ISaveGame
        {
            return ServiceLocator.ServiceLocator.Instance?.Get<T>();
        }

        private string GenerateUniqueKeyForService(ISaveGame service)
        {
            return GetDataTypeSaveName(service) + GetDataNameSaveName(service);
        }
    
        private string GetDataTypeSaveName(ISaveGame service)
        {
            // Example: Use service type name + unique identifier
            return service.GetType().Name; // Customize as needed
        }
    
        private string GetDataNameSaveName(ISaveGame service)
        {
            // Example: Use service type name + unique identifier
            return service.ToString(); // Customize as needed
        }

    }
}


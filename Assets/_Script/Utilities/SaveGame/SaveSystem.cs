using System;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Utilities.SaveGame
{
    public class SaveSystem : Singleton<SaveSystem>
    {
        [SerializeField] private bool _debug = false;

        public bool LoadData<T>(string saveName) where T : ISaveGame
        {
            if (IsNewSave<T>(saveName))
            {
                Debug.Log($"It is a new save: {GetInternalName<T>(saveName)}");
                LoadDefaultData<T>();
                return false;
            }
            else
            {
                Debug.Log($"Loading saved data for {GetInternalName<T>(saveName)}");
                Load<T>(saveName);
                return true;
            }
        }
        private string GetInternalName<T>(string saveName)
        {
            return $"{saveName}_{typeof(T).Name}";
        }

        private bool IsNewSave<T>(string saveName) where T : ISaveGame
        {
            return !ES3.KeyExists(GetInternalName<T>(saveName));
        }

        private void LoadDefaultData<T>() where T : ISaveGame
        {
            Debug.Log($"Target {typeof(T)} has no initial data. Loading default data from scriptableObject.");
            T saveService = GetSaveGameService<T>();
            saveService?.LoadDefaultData();
        }

        public void SaveData<T>(string saveName) where T : ISaveGame
        {
            T saveService = GetSaveGameService<T>();
            if (saveService == null)
            {
                Debug.LogWarning($"No service found for {typeof(T).Name}. Data will not be saved.");
                return;
            }

            string key = GenerateUniqueKeyForService(saveService); // Generate unique key
            object data = saveService.OnSaveData();
            ES3.Save(key, data); // Save the data
            
            if (_debug)
                Debug.Log($"Saving... {saveName}_{typeof(T).Name}");
            ES3.Save(GetInternalName<T>(saveName), key); // Save the unique key
        }

        private void Load<T>(string saveName) where T : ISaveGame
        {
            string typeSpecificSaveName = $"{saveName}_{typeof(T).Name}";

            if (!ES3.KeyExists(typeSpecificSaveName))
            {
                Debug.LogWarning($"No saved data found for {typeSpecificSaveName}");
                return;
            }
            
            string key = ES3.Load<string>(typeSpecificSaveName);
            T saveService = GetSaveGameService<T>();
            if (saveService != null && ES3.KeyExists(key))
            {
                object data = ES3.Load<object>(key); // Consider specifying a more specific type if possible
                saveService.OnLoadData(data);
            }
        }

        private T GetSaveGameService<T>() where T : ISaveGame
        {
            return ServiceLocator.ServiceLocator.Instance.Get<T>();
        }

        private string GenerateUniqueKeyForService(ISaveGame service)
        {
            return $"{service.GetType().Name}_{service.SaveKey}";
        }

        private string GetDataTypeSaveName(ISaveGame service)
        {
            return service.GetType().Name; // Customize as needed
        }

        private string GetDataNameSaveName(ISaveGame service)
        {
            return service.ToString(); // Customize as needed
        }
    }
}

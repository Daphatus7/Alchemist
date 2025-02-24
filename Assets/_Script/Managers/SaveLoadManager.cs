using _Script.Utilities.SaveGame;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Managers
{
    public class SaveLoadManager: PersistentSingleton<SaveLoadManager>
    {
        [SerializeField] private string saveName = "save_1";
        
        private SaveSystem SaveSystem => SaveSystem.Instance;
        
        
        //Reference to lower level managers
        
        //Supports they have registered with the service locator
        
        [Button]
        public void SaveGame()
        {
            SaveTownData();
        }


        
        [Button]
        public void LoadGame()
        {
            LoadTownData();
        }
        
        
        public void SaveTownData()
        {
            SaveSystem.SaveData<ISaveTownDataHandler>(saveName);
        }
        
        public void LoadTownData()
        {
            SaveSystem.LoadData<ISaveTownDataHandler>(saveName);
        }
        
   
        
        [Button]
        public void DeleteSave()
        {
            // Delete the unified save file.
            string filePath = Application.persistentDataPath + "/" + saveName + ".es3";
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                Debug.Log("Deleted save file: " + filePath);
            }
            else
            {
                Debug.Log("No save file found at: " + filePath);
            }
        }
    }
}
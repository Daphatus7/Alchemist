using System;
using System.Collections;
using _Script.Map;
using _Script.Utilities.SaveGame;
using _Script.Utilities.ServiceLocator;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

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
            SaveSystem.Instance.SaveData<ISaveGameManager>(saveName);
        }
        
        [Button]
        public void LoadGame()
        {
            // load different components of the game data and to be loaded separately
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
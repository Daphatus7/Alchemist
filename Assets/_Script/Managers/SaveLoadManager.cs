using System;
using System.Collections;
using _Script.Map;
using _Script.Utilities.SaveGame;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Script.Managers
{
    public class SaveLoadManager: PersistentSingleton<SaveLoadManager>
    {
        [SerializeField] private string saveName = "save_1";
        [FormerlySerializedAs("_removeSave")] [SerializeField] private bool removeSave = false;
        [FormerlySerializedAs("_gridTester")] [SerializeField] private GameTileMap gridTester;
        
        [SerializeField] private bool _debug = false;
        
        public void Start()
        {
            //StartCoroutine(DelayedLoad());
        }
        
        private IEnumerator DelayedLoad()
        {
            yield return new WaitForSeconds(0.5f);
            if (LoadSavedTileMap())
            {
                if(_debug)
                    Debug.Log("Loaded saved tile map...");
            }
            else
            {
                if(_debug)
                    Debug.Log("Failed to load saved tile map...");
                LoadDefaultTileMap();
            }
            
            StartCoroutine(SaveGameRoutine());
        }
        
        private IEnumerator SaveGameRoutine()
        {
            yield return new WaitForSeconds(3f);
            SaveGame();
        }
        
        public void SaveGame()
        {
            if(_debug)
                Debug.Log("Saving game...");
            SaveSystem.Instance.SaveData<ISaveTileMap>(saveName);
        }
        
        public bool LoadSavedTileMap()
        {
            if(_debug)
                Debug.Log("Trying to load saved tile map...");
            return SaveSystem.Instance.LoadData<ISaveTileMap>(saveName);
        }
        
        public void RemoveSave()
        {
            
        }
        
        public void LoadDefaultTileMap()
        {
            gridTester.LoadDefaultData();
        }
    }
}
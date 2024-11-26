using System;
using System.Collections;
using _Script.Map;
using _Script.Utilities.SaveGame;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Managers
{
    public class SaveLoadManager: PersistentSingleton<SaveLoadManager>
    {
        [SerializeField] private string saveName = "save_1";
        [SerializeField] private bool _removeSave = false;
        
        [SerializeField] private GenericGridTester _gridTester;
        
        public void Start()
        {
            StartCoroutine(DelayedLoad());
        }
        
        private IEnumerator DelayedLoad()
        {
            yield return new WaitForSeconds(0.5f);
            if (LoadSavedTileMap())
            {
                Debug.Log("Loaded saved tile map...");
            }
            else
            {
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
            Debug.Log("Saving game...");
            SaveSystem.Instance.SaveData<ISaveTileMap>(saveName);
        }
        
        public bool LoadSavedTileMap()
        {
            Debug.Log("Trying to load saved tile map...");
            return SaveSystem.Instance.LoadData<ISaveTileMap>(saveName);
        }
        
        public void RemoveSave()
        {
            
        }
        
        public void LoadDefaultTileMap()
        {
            _gridTester.LoadDefaultData();
        }
    }
}
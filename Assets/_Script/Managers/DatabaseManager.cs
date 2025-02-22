// Author : Peiyu Wang @ Daphatus
// 16 02 2025 02 51

using System.Collections.Generic;
using _Script.Enemy.EnemyDatabase;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Items.Lootable;
using _Script.Managers.Database;
using _Script.Managers.Database._Script.Managers.Database;
using UnityEngine;

namespace _Script.Managers
{
    public class DatabaseManager : PersistentSingleton<DatabaseManager>
    {
        [SerializeField] private EnemyDatabase _enemyDatabase;
        [SerializeField] private ItemDatabase _itemDatabase;
        
        private Dictionary<string, ItemData> _itemDictionary;
        private Dictionary<string, GameObject> _enemyPrefabDictionary;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Initialize the enemy prefab dictionary from the enemy database asset.
            if (_enemyDatabase != null)
            {
                _enemyPrefabDictionary = CreateEnemyDictionary();
            }
            else
            {
                Debug.LogError("DatabaseManager: EnemyDatabase asset is null.");
            }
            
            if (_itemDatabase != null)
            {
                _itemDictionary = CreateItemDictionary();
            }
            else
            {
                Debug.LogError("DatabaseManager: ItemDatabase asset is null.");
            }
        }
        
        /// <summary>
        /// Retrieves an enemy prefab by its name.
        /// </summary>
        public GameObject GetEnemyPrefab(string enemyName)
        {
            if (!_enemyDatabase)
            {
                Debug.LogError("DatabaseManager: EnemyDatabase is null.");
                return null;
            }
            
            // Ensure the enemy dictionary is initialized.
            if (_enemyPrefabDictionary == null)
            {
                _enemyPrefabDictionary = CreateEnemyDictionary();
            }
            
            if (_enemyPrefabDictionary.TryGetValue(enemyName, out var enemyPrefab))
            {
                return enemyPrefab;
            }
            
            Debug.LogError("DatabaseManager: EnemyDatabase does not contain enemy: " + enemyName);
            return null;
        }
        
        /// <summary>
        /// Retrieves the item data corresponding to the given item ID.
        /// </summary>
        public ItemData GetItemData(string itemName)
        {
            if (!_itemDatabase)
            {
                Debug.LogError("DatabaseManager: ItemDatabase is null.");
                return null;
            }
            if (_itemDictionary.TryGetValue(itemName, out var item))
            {
                return item;
            }
            Debug.LogError("DatabaseManager: ItemDatabase does not contain item: " + itemName);
            return null;
        }
        
        /// <summary>
        /// Builds a dictionary mapping enemy names to their corresponding prefabs from the enemy database asset.
        /// </summary>
        private Dictionary<string, GameObject> CreateEnemyDictionary()
        {
            var dict = new Dictionary<string, GameObject>();
            var pairs = _enemyDatabase.GetEnemyDataPairs();
            
            if (pairs == null || pairs.Count == 0)
            {
                Debug.LogWarning("EnemyDatabaseRuntime: No enemy data pairs available.");
                return dict;
            }
            
            foreach (var pair in pairs)
            {
                if (pair == null || string.IsNullOrEmpty(pair.enemyName) || pair.enemyPrefab == null)
                {
                    Debug.LogWarning("EnemyDatabaseRuntime: Encountered an invalid enemy data pair.");
                    continue;
                }
                
                if (!dict.ContainsKey(pair.enemyName))
                {
                    dict.Add(pair.enemyName, pair.enemyPrefab);
                }
                else
                {
                    Debug.LogWarning("EnemyDatabaseRuntime: Duplicate enemy name detected: " + pair.enemyName);
                }
            }
            
            return dict;
        }
        
        private Dictionary<string, ItemData> CreateItemDictionary()
        {
            var dict = new Dictionary<string, ItemData>();
            var items = _itemDatabase.Items;
            foreach(var item in items)
            {
                Debug.Log(item.itemData.itemID + " x " + item.itemData.itemName);
               dict.Add(item.itemData.itemID, item.itemData);
            }
            return dict;
        }
    }
}
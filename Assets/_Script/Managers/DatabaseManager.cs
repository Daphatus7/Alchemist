// Author : Peiyu Wang @ Daphatus
// 16 02 2025 02 51

using System.Collections.Generic;
using _Script.Enemy.EnemyDatabase;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Managers.Database;
using UnityEngine;

namespace _Script.Managers
{
    public class DatabaseManager : PersistentSingleton<DatabaseManager>
    {
        [SerializeField] private ItemDatabase _itemDatabase;
        [SerializeField] private EnemyDatabase _enemyDatabase;
        
        private Dictionary<string, ItemData> _itemDataDictionary;
        private Dictionary<string, GameObject> _enemyPrefabDictionary;
        
        public GameObject GetEnemyPrefab(string enemyName)
        {
            if (!_enemyDatabase)
            {
                Debug.LogError("DatabaseManager: EnemyDatabase is null.");
                return null;
            }

            if (_enemyPrefabDictionary == null)
            {
                _enemyPrefabDictionary = CreateEnemyDictionary();
                if(_enemyPrefabDictionary.TryGetValue(enemyName, out var enemyPrefab))
                {
                    return enemyPrefab;
                }
                else
                {
                    Debug.LogError("DatabaseManager: EnemyDatabase does not contain enemy: " + enemyName);
                }
            }
            
            if(_enemyPrefabDictionary.TryGetValue(enemyName, out var enemyPrefab2))
            {
                return enemyPrefab2;
            }
            
            
            Debug.LogError("DatabaseManager: EnemyDatabase does not contain enemy: " + enemyName);
            return null;

        }
        
        public ItemData GetItemData(string itemName)
        {
            if (!_itemDatabase)
            {
                Debug.LogError("DatabaseManager: ItemDatabase is null.");
                return null;
            }
            
            var itemData = _itemDatabase.SearchItemByID(itemName);
            if (itemData != null) return itemData;
            Debug.LogError("DatabaseManager: ItemDatabase does not contain item: " + itemName);
            return null;
        }
        /// <summary>
        /// Creates the runtime dictionary from the enemy data pairs stored in the asset.
        /// </summary>
        /// <returns>A dictionary mapping enemy names to their corresponding prefabs.</returns>
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
    }
}
// Author : Peiyu Wang @ Daphatus
// 16 02 2025 02 51

using _Script.Enemy.EnemyDatabase;
using _Script.Managers.Database;
using UnityEngine;

namespace _Script.Managers
{
    public class DatabaseManager : PersistentSingleton<DatabaseManager>
    {
        [SerializeField] private ItemDatabase _itemDatabase;
        [SerializeField] private EnemyDatabase _enemyDatabase;
        
        public GameObject GetEnemyPrefab(string enemyName)
        {
            if (_enemyDatabase == null)
            {
                Debug.LogError("DatabaseManager: EnemyDatabase is null.");
                return null;
            }

            if (_enemyDatabase.GetEnemyPrefab(enemyName) == null)
            {
                Debug.LogError("DatabaseManager: EnemyDatabase does not contain enemy: " + enemyName);
                return null;
            }

            return _enemyDatabase.GetEnemyPrefab(enemyName);
        }
    }
}
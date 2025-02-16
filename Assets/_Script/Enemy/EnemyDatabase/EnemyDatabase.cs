// Author : Peiyu Wang @ Daphatus
// 16 02 2025 02 19

using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Script.Enemy.EnemyDatabase
{
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Enemy/Enemy Database", order = 1)]
    [Title("Enemy Database")]
    [Serializable]
    public class EnemyDatabase : SerializedScriptableObject
    {
        #region Settings

        [BoxGroup("Settings")]
        [FolderPath(AbsolutePath = true, RequireExistingPath = true)]
        [ValidateInput("IsFolderPathValid", "Folder path must not be empty", InfoMessageType.Error)]
        [Tooltip("Path to the folder that holds enemy prefabs.\n" +
                 "Example: 'Assets/_Prefabs/Enemy'")]
        [SerializeField]
        private string enemyFolderPath = "Assets/_Prefabs/Enemy";

        #endregion

        // This list is populated with enemy prefabs (enemy name + prefab) and is displayed in the Inspector.
        [ShowInInspector, ReadOnly, BoxGroup("Scanned Enemies"), ListDrawerSettings(Expanded = true)]
        [SerializeField]
        private List<EnemyDataPair> enemyDataPairs = new List<EnemyDataPair>();

        // This dictionary is used at runtime for quick lookups.
        private Dictionary<string, GameObject> _enemyDatabase = new Dictionary<string, GameObject>();
        /// <summary>
        /// Builds the runtime dictionary from the serialized enemyDataPairs.
        /// </summary>
        /// <returns>A dictionary mapping enemy names to their corresponding prefabs.</returns>
        private Dictionary<string, GameObject> CreateEnemyDatabaseFromDataPairs()
        {
            var database = new Dictionary<string, GameObject>();

            foreach (var pair in enemyDataPairs)
            {
                if (pair.enemyPrefab == null)
                {
                    Debug.LogWarning("EnemyDatabase: Encountered a null prefab for enemy: " + pair.enemyName);
                    continue;
                }

                if (!database.ContainsKey(pair.enemyName))
                {
                    database.Add(pair.enemyName, pair.enemyPrefab);
                }
                else
                {
                    Debug.LogWarning("EnemyDatabase: Duplicate enemy name detected: " + pair.enemyName);
                }
            }

            if (database.Count == 0)
            {
                Debug.LogWarning("EnemyDatabase: No enemy prefabs found in the scanned data pairs.");
            }

            return database;
        }

        /// <summary>
        /// Retrieves the enemy prefab associated with the given enemy name.
        /// </summary>
        /// <param name="enemyName">The key (name) of the enemy prefab.</param>
        /// <returns>The enemy prefab if found; otherwise, null.</returns>
        public GameObject GetEnemyPrefab(string enemyName)
        {
            if (_enemyDatabase.TryGetValue(enemyName, out GameObject prefab))
            {
                return prefab;
            }

            Debug.LogError($"EnemyDatabase: Enemy prefab not found for key: {enemyName}");
            return null;
        }

        /// <summary>
        /// Returns the serialized list of enemy data pairs.
        /// </summary>
        public List<EnemyDataPair> GetEnemyDataPairs()
        {
            return enemyDataPairs;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Scans the specified folder using the AssetDatabase (Editor-only) and populates the enemyDataPairs list.
        /// </summary>
        [BoxGroup("Actions")]
        [Button("Scan For Enemies", ButtonSizes.Large)]
        private void ScanForEnemies()
        {
            enemyDataPairs.Clear();

            // Find all assets of type GameObject (prefabs) in the specified folder.
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { enemyFolderPath });
            Debug.Log($"Scanning folder: {enemyFolderPath}. Found {guids.Length} assets.");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (enemyPrefab == null)
                {
                    Debug.LogWarning("EnemyDatabase: Encountered a null prefab at path: " + path);
                    continue;
                }

                // Use the prefab's name as the key.
                string enemyName = enemyPrefab.name;
                enemyDataPairs.Add(new EnemyDataPair
                {
                    enemyName = enemyName,
                    enemyPrefab = enemyPrefab
                });
            }

            _enemyDatabase = CreateEnemyDatabaseFromDataPairs();
            Debug.Log("EnemyDatabase: Scanned and updated enemy database.");
            EditorUtility.SetDirty(this); // Mark the asset as dirty so changes are saved.
        }
#endif

        #region Odin Validation

        private bool IsFolderPathValid(string path)
        {
            return !string.IsNullOrEmpty(path);
        }

        #endregion
    }

    [Serializable]
    public class EnemyDataPair
    {
        [LabelText("Enemy Name")]
        public string enemyName;
        [LabelText("Enemy Prefab")]
        public GameObject enemyPrefab;
    }
}
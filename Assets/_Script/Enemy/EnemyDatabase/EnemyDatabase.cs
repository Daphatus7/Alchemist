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
        [Tooltip("Path to the folder that holds enemy prefabs.\nExample: 'Assets/_Prefabs/Enemy'")]
        [SerializeField]
        private string enemyFolderPath = "Assets/_Prefabs/Enemy";

        #endregion

        // This list is populated with enemy prefabs (enemy name + prefab) and is displayed in the Inspector.
        [ShowInInspector, ReadOnly, BoxGroup("Scanned Enemies"), ListDrawerSettings(Expanded = true)]
        [SerializeField]
        private List<EnemyDataPair> enemyDataPairs = new List<EnemyDataPair>();

        /// <summary>
        /// Returns the list of enemy data pairs.
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
            if (string.IsNullOrEmpty(enemyFolderPath))
            {
                Debug.LogError("EnemyDatabaseAsset: Enemy folder path is not set.");
                return;
            }

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
                    Debug.LogWarning("EnemyDatabaseAsset: Encountered a null prefab at path: " + path);
                    continue;
                }

                // Use the prefab's name as the key.
                string enemyName = enemyPrefab.name;
                if (string.IsNullOrEmpty(enemyName))
                {
                    Debug.LogWarning("EnemyDatabaseAsset: Encountered a prefab with an empty name at path: " + path);
                    continue;
                }

                enemyDataPairs.Add(new EnemyDataPair
                {
                    enemyName = enemyName,
                    enemyPrefab = enemyPrefab
                });
            }

            Debug.Log("EnemyDatabaseAsset: Scanned and updated enemy database.");
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
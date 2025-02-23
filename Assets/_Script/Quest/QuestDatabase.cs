// Author : Peiyu Wang @ Daphatus
// Updated: [Current Date]

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace _Script.Quest
{
    [CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Quest Database", order = 1)]
    [Title("Quest Database")]
    [Serializable]
    public class QuestDatabase : SerializedScriptableObject
    {
        #region Settings

        [BoxGroup("Settings")]
        [FolderPath(AbsolutePath = true, RequireExistingPath = true)]
        [ValidateInput("IsFolderPathValid", "Folder path must not be empty", InfoMessageType.Error)]
        [Tooltip("Path to the folder that holds quest definitions (SimpleQuestDefinition assets).\nExample: 'Assets/Quests'")]
        [SerializeField]
        private string questFolderPath = "Assets/_Prefabs/Quest";

        #endregion

        // List of quest definitions loaded from the folder.
        [BoxGroup("Current Database Content")]
        [LabelText("Quests in Database")]
        [TableList(AlwaysExpanded = true)]
        [InfoBox("List of all SimpleQuestDefinition assets loaded from the specified folder.", InfoMessageType.Info)]
        [SerializeField]
        private List<QuestDefinition.SimpleQuestDefinition> questDefinitions = new List<QuestDefinition.SimpleQuestDefinition>();
        public List<QuestDefinition.SimpleQuestDefinition> QuestDefinitions => questDefinitions;
#if UNITY_EDITOR
        /// <summary>
        /// Scans the specified folder using the AssetDatabase (Editor-only) and populates the questDefinitions list.
        /// </summary>
        [BoxGroup("Actions")]
        [Button("Scan For Quests", ButtonSizes.Large)]
        private void ScanForQuests()
        {
            if (string.IsNullOrEmpty(questFolderPath))
            {
                Debug.LogError("QuestDatabase: Quest folder path is not set.");
                return;
            }

            questDefinitions.Clear();

            // Find all assets of type SimpleQuestDefinition in the specified folder.
            string[] guids = AssetDatabase.FindAssets("t:SimpleQuestDefinition", new string[] { questFolderPath });
            Debug.Log($"Scanning folder: {questFolderPath}. Found {guids.Length} quest assets.");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                QuestDefinition.SimpleQuestDefinition questDef = AssetDatabase.LoadAssetAtPath<QuestDefinition.SimpleQuestDefinition>(path);

                if (questDef == null)
                {
                    Debug.LogWarning("QuestDatabase: Encountered a null quest asset at path: " + path);
                    continue;
                }

                questDefinitions.Add(questDef);
            }

            Debug.Log("QuestDatabase: Scanned and updated quest database.");
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
}
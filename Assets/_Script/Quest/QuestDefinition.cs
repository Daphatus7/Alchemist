// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 30

using System;
using UnityEngine;

namespace _Script.Quest
{
    
    /// <summary>
    /// Scriptable object that defines a single quest
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        public string questID;
        public string questName;
        public string questDescription;
        public NpcDialogue questStartDialogue;
        public NpcDialogue questEndDialogue;
        public QuestObjective[] objectives; // Array of objectives
        public QuestReward reward;          // Could be items, gold, exp, etc.
        
        
        private void OnValidate()
        {
            #if UNITY_EDITOR
            questID = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
    
    [Serializable]
    public class NpcDialogue
    {
        public string [] dialogue;
        public static string defaultDialogue = "Hello, I have a quest for you!";
    }
}
// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 30

using System;
using _Script.Character.PlayerRank;
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
        public NpcDialogue questStartDialogue;
        public NpcDialogue questInProgressDialogue;
        public NpcDialogue questCompleteDialogue;
        public UnlockCondition unlockCondition; // Prerequisite to unlock this quest
        public QuestObjective[] objectives; // Array of objectives
        public QuestReward reward;          // Could be items, gold, exp, etc.
        
        private void OnValidate()
        {
            #if UNITY_EDITOR
            questID = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        public bool CanUnlockQuest()
        {
            return unlockCondition == null || QuestManager.Instance.CheckPrerequisite(unlockCondition);
        }
    }

    [Serializable]
    public class UnlockCondition
    {
        public PlayerRankEnum playerRank;
        public MainStoryLine prerequisite;
    }
    
    [Serializable]
    public class NpcDialogue
    {
        public string [] dialogue;
        public static string defaultDialogue = "Hello, I have a quest for you!";
    }
}
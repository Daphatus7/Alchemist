// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 30

using System;
using _Script.Character.PlayerRank;
using UnityEngine;

namespace _Script.Quest.QuestDefinition
{
    
    /// <summary>
    /// Scriptable object that defines a single quest
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest Definition")]
    public class QuestDefinition : SimpleQuestDefinition
    {
        public NpcDialogue questStartDialogue;
        public NpcDialogue questInProgressDialogue;
        public NpcDialogue questCompleteDialogue;
        public UnlockCondition unlockCondition; // Prerequisite to unlock this quest
        public bool CanUnlockQuest()
        {
            return unlockCondition == null || QuestManager.Instance.CheckPrerequisite(unlockCondition);
        }
    }
    
    

    [Serializable]
    public class UnlockCondition
    {
        public NiRank playerRank;
        public MainStoryLine prerequisite;
    }
    
    [Serializable]
    public class NpcDialogue
    {
        public string [] dialogue;
        public static string defaultDialogue = "Hello, I have a quest for you!";
    }
}
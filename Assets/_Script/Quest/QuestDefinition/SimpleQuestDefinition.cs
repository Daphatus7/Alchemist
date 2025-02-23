// Author : Peiyu Wang @ Daphatus
// 09 02 2025 02 52

using _Script.Quest.QuestInstance;
using UnityEngine;

namespace _Script.Quest.QuestDefinition
{
    public abstract class SimpleQuestDefinition : ScriptableObject
    {
        public string questID;
        public string questName;
        public QuestObjective[] objectives; // Array of objectives
        public QuestReward reward;          // Could be items, gold, exp, etc.
        public abstract QuestType QuestType { get; }
        private void OnValidate()
        {
#if UNITY_EDITOR
            questID = this.name;
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
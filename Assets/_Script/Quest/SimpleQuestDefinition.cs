// Author : Peiyu Wang @ Daphatus
// 09 02 2025 02 52

using UnityEngine;

namespace _Script.Quest
{
    public abstract class SimpleQuestDefinition : ScriptableObject
    {
        public string questID;
        public string questName;
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
}
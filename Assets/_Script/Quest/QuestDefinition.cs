// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 30

using UnityEngine;

namespace _Script.Quest
{
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        public string questID;
        public string questName;
        public string questDescription;
    
        public QuestObjective[] objectives; // Array of objectives
        public QuestReward reward;          // Could be items, gold, exp, etc.
    }
}
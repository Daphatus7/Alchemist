// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 46

using System;
using UnityEngine;

namespace _Script.Quest
{
    public class QuestObjective
    {
        public ObjectiveType objectiveType;
        public string targetID;
        private int _requiredCount; public int RequiredCount => _requiredCount;
        private int _currentCount; public int CurrentCount => _currentCount;
        private string _npcID;
        public bool isComplete;
        public event Action OnObjectiveUpdate;

        public QuestState ParentQuestState;

        public void StartTracking()
        {
            
        }

        public void StopTracking()
        {
           
        }

        private void OnItemCollected(string itemID)
        {
           
        }

        private void OnEnemyKilled(string enemyID)
        {
            
        }
    }

    public enum ObjectiveType
    {
        Kill,
        Collect
    }
    
    [Serializable]
    public class QuestReward
    {
        public int gold;
        public int experience;
        public string [] itemIDs; // IDs of items to give as a reward
    }
}
// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 14

using System;
using System.Collections.Generic;
using _Script.Character.PlayerRank;
using UnityEngine;
namespace _Script.Quest
{
    public class QuestManager : Singleton<QuestManager>
    {
        // All quests the player has accepted (or is able to track).
        private readonly List<QuestState> _activeQuests = new List<QuestState>();
        
        private Dictionary<PlayerRank, List<SideQuest>> _sideQuests = new Dictionary<PlayerRank, List<SideQuest>>();

        public event Action<QuestState> OnQuestUpdate;
        
        #region sideQuests
        
        public void AddSideQuest(PlayerRank rank, SideQuest quest)
        {
            if (!_sideQuests.ContainsKey(rank))
            {
                _sideQuests.Add(rank, new List<SideQuest>());
            }
            _sideQuests[rank].Add(quest);
        }
        
        public void RemoveSideQuest(PlayerRank rank, SideQuest quest)
        {
            if (!_sideQuests.ContainsKey(rank))
            {
                return;
            }
            _sideQuests[rank].Remove(quest);
        }
        
        public List<SideQuest> GetSideQuests(PlayerRank rank)
        {
            if (!_sideQuests.ContainsKey(rank))
            {
                return new List<SideQuest>();
            }
            return _sideQuests[rank];
        }

        #endregion
        
        
        // Called when player accepts a new quest
        public void StartQuest(QuestDefinition questDef)
        {
            // Check if already active
            if (_activeQuests.Exists(q => q.Definition == questDef))
            {
                Debug.Log($"{questDef.questName} is already active.");
                return;
            }
            //
            // QuestState newQuest = new QuestState(questDef);
            // _activeQuests.Add(newQuest);
            // Debug.Log($"Quest started: {questDef.questName}");
        }

        // Called when the player picks up an item
        public void OnItemCollected(string itemID)
        {
        }
        // Called when an enemy is killed
        public void OnEnemyKilled(string enemyID)
        {
           
        }
        private void CheckQuestCompletion(QuestState quest)
        {
            // // If all objectives complete, the quest is done
            // bool allComplete = true;
            // foreach (QuestObjective obj in quest.runtimeObjectives)
            // {
            //     if (!obj.isComplete)
            //     {
            //         allComplete = false;
            //         break;
            //     }
            // }
            //
            // if (allComplete)
            // {
            //     quest.isCompleted = true;
            //     Debug.Log($"Quest completed: {quest.Definition.questName}");
            //     GiveReward(quest.Definition.reward);
            //     // If you want to remove from active or keep it in a "completed" list:
            //     // _activeQuests.Remove(quest);
            // }
        }
        private void GiveReward(QuestReward reward)
        {
            // In a real game, you'd add gold, experience, items to the player's inventory.
            // For demonstration:
            Debug.Log($"Reward granted: {reward.gold} gold, {reward.experience} XP");
            foreach (var itemID in reward.itemIDs)
            {
                Debug.Log($"Item received: {itemID}");
            }
        }
    }
}
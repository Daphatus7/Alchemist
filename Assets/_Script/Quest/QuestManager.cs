// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 14

using System;
using System.Collections.Generic;
using _Script.Items.Lootable;
using _Script.Managers;
using _Script.Map;
using _Script.NPC.NpcBackend;
using _Script.NPC.NpcBackend.NpcModules;
using _Script.Quest.PlayerQuest;
using _Script.Utilities.ServiceLocator;
using UnityEditor.PackageManager;
using UnityEngine;

namespace _Script.Quest
{
    
    public sealed class QuestManager : Singleton<QuestManager>
    {
        /// <summary>
        /// NPC ID -> Quest Giver Module
        /// </summary>
        private Dictionary<string, INpcQuestModuleHandler> _questModules;
        public event Action<string> onEnemyKilled;
        public event Action<string, int> onItemCollected;
        public event Action<string> onAreaEntered;
        public event Action<QuestInstance> onQuestCompleted;

        [SerializeField] public StorylineChecker storylineChecker;
        
        public void Start()
        {
            InitializeNpcModules();
        }
        
        /// <summary>
        /// Collect all the quest giver modules
        /// </summary>
        private void InitializeNpcModules()
        {
            _questModules = new Dictionary<string, INpcQuestModuleHandler>();
            
            var questModules = FindObjectsByType<QuestGiverModule>(FindObjectsSortMode.None);
            foreach (var o  in questModules)
            {
                if (_questModules.ContainsKey(o.NpcId))
                {
                    continue;
                }
                _questModules.Add(o.NpcId, o);
            }
        }
        
        
        /// <summary>
        /// this could be called when certain event happens
        /// </summary>
        private void UpdateNpcModules()
        {
            foreach (var module in _questModules)
            {
                module.Value.TryUnlockQuest();
            }
        }
        

        /// <summary>
        /// Triggered when an item is collected
        /// Should be called by the inventory manager
        /// </summary>
        /// <param name="itemID"> item ID</param>
        /// <param name="totalCount"> the number of items in the inventory</param>
        public void OnItemCollected(string itemID, int totalCount)
        {
            onItemCollected?.Invoke(itemID,totalCount);
        }

        /// <summary>
        /// Happens when an enemy is killed
        /// </summary>
        /// <param name="enemyID"></param>
        public void OnEnemyKilled(string enemyID)
        {
            onEnemyKilled?.Invoke(enemyID);
        }
        
        public void OnEnteringArea(string areaID)
        {
            onAreaEntered?.Invoke(areaID);
        }

        public bool CheckQuestCompletion(QuestInstance quest)
        {
            // Iterate over each objective in the quest.
            foreach (var objective in quest.Objectives)
            {
                // If any objective is not marked as complete, the quest is not complete.
                if (!objective.isComplete)
                {
                    Debug.Log($"Objective {objective.objectiveData.Type} is not complete");
                    return false;
                }

                // If the objective is a "Collect" type, verify the inventory count.
                if (objective.objectiveData.Type == ObjectiveType.Collect)
                {
                    var collectObjective = (CollectObjective)objective.objectiveData;
                    var count = GameManager.Instance.PlayerCharacter.PlayerInventory.GetItemCount(collectObjective.item.itemID);
                    // If the inventory count is less than required, the objective is not satisfied.
                    if (count < objective.objectiveData.requiredCount)
                    {
                        return false;
                    }
                }
        
                // For other objective types, you might add additional checks here if needed.
            }
    
            // If we get through all objectives without returning false, then the quest is complete.
            return true;
        }
        
        public void CompleteQuest(string questId)
        {
            
        }
        
        public void CompleteQuest(QuestInstance currentQuest)
        {
            var questObjectives = currentQuest.Objectives;
            //check if the quest is completed
            foreach (var o in questObjectives)
            {
                if (o.isComplete)
                {
                    if (o.objectiveData.Type == ObjectiveType.Collect)
                    {
                        //remove the items from the player inventory
                        GameManager.Instance.PlayerCharacter.PlayerInventory.RemoveItemById(((CollectObjective)o.objectiveData).item.itemID, o.objectiveData.requiredCount);
                    }
                }
                else
                {
                    throw new Exception("Quest is not completed, but it was claimed to be completed");
                    return;
                }
            }
            GiveReward(currentQuest.QuestDefinition.reward);
            ServiceLocator.Instance.Get<IPlayerQuestService>().CompleteQuest(currentQuest);
            currentQuest.Cleanup();
        }
        
        private void GiveReward(QuestReward reward)
        {
            //Drop the reward items
            foreach (var o in reward.items)
            {
                //Debug the item drop
                Debug.Log($"[QuestManager] Dropping {o.amount} {o.item.itemID}");
                ItemLootable.DropItem(GameManager.Instance.PlayerCharacter.transform.position, o.item, o.amount);
            }
        }

        public bool CheckPrerequisite(UnlockCondition prerequisite)
        {
            //check if the player has the required rank
            return GameManager.Instance.PlayerRank >= prerequisite.playerRank &&
                   //check if the player has completed the required quest
                   storylineChecker.IsCompleted(prerequisite.prerequisite);
        }

        private void OnQuestCompleted(QuestInstance quest)
        {
            onQuestCompleted?.Invoke(quest);
        }


        #region Guild Quests

        private GuildQuestInstance _currentQuest;
        
        public bool CreateGuildQuest(GuildQuestDefinition quest)
        {
            //check if there is an existing quest
            if(_currentQuest != null)
            {

                switch (_currentQuest.QuestState)
                {
                    case QuestState.Completed:
                        //if the quest is completed, create a new quest
                        throw new Exception("Quest is completed, but a new quest is being created, should not enter this menu");
                    case QuestState.InProgress:
                        Debug.Log("Quest is in progress, cannot create a new quest potential add a new UI to inform the player");
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                //Create an active quest
                _currentQuest = new GuildQuestInstance(quest);
                
                
                //Update UI
                ServiceLocator.Instance.Get<IPlayerQuestService>().AddNewGuildQuest(_currentQuest);
                
                //Generate path
                MapController.Instance.GeneratePathForQuest(_currentQuest);
            }
            return true;
        }

        public void CompleteGuildQuest(string questId)
        {
            if (_currentQuest == null)
            {
                Debug.Log("No quest to complete");
                return;
            }
            if (_currentQuest.QuestDefinition.questID == questId)
            {
                if (CheckQuestCompletion(_currentQuest))
                {
                    CompleteQuest(_currentQuest);
                    _currentQuest = null;
                }
                else
                {
                    Debug.Log("Quest is not completed");
                }
            }
            else
            {
                Debug.Log("Quest ID does not match the current quest");
            }
        }
        
        public void CompleteGuildQuest()
        {
            if (_currentQuest == null)
            {
                Debug.Log("No quest to complete");
                return;
            }
            if (CheckQuestCompletion(_currentQuest))
            {
                CompleteQuest(_currentQuest);
                _currentQuest = null;
            }
            else
            {
                Debug.Log("Quest is not completed");
            }
        }
        
        #endregion

    }

}
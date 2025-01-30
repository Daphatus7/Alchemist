// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 43

using System;
using System.Collections.Generic;

using System.Collections.Generic;
using UnityEngine;

namespace _Script.Quest
{
    /// <summary>
    /// Player holding instance
    /// </summary>
    public abstract class QuestInstance
    {
        private readonly QuestDefinition _definition; public QuestDefinition QuestDefinition => _definition;
        private QuestState _state; public QuestState State => _state;
        private readonly List<QuestObjective> _objectives = new List<QuestObjective>(); public List<QuestObjective> Objectives => _objectives;
        
        public QuestInstance(QuestDefinition def)
        {
            _definition = def;
            _state = QuestState.Active;
            // For each static ObjectiveData, create a dynamic QuestObjective
            if (QuestManager.Instance != null)
            {
                foreach (var objData in _definition.objectives)
                {
                    var questObj = new QuestObjective(objData.objectiveData);
                    _objectives.Add(questObj);
                    switch (objData.objectiveData.type)
                    {
                        case ObjectiveType.Kill:
                            QuestManager.Instance.onEnemyKilled += OnEnemyKilled;
                            break;
                        case ObjectiveType.Collect:
                            QuestManager.Instance.onItemCollected += OnItemCollected;
                            break;
                        case ObjectiveType.Explore:
                            Debug.Log("Exploration objective detected");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        
        
        /// <summary>
        /// Called by the quest manager when the player interacts with the quest giver
        /// Check if the quest is complete
        /// Check inventory for items etc.
        /// </summary>
        public bool TryCompleteQuest()
        {
            CheckCompletion();
            return false;
        }
        private void OnItemCollected(string itemID, int totalCount)
        {
            _objectives.ForEach(obj =>
            {
                // Skip if the objective is not a collect objective
                if (obj.objectiveData.type != ObjectiveType.Collect) return;
                
                var collectObj = (CollectObjective) obj.objectiveData;
                
                // Skip if the collected item is not the required item
                if (collectObj.item.itemID != itemID) return;
                obj.currentCount = totalCount;
                if (obj.currentCount >= obj.objectiveData.requiredCount)
                {
                    obj.isComplete = true;
                    //considered as completed now, but if the items are removed, the objective will be considered as not completed
                }
                else
                {
                    obj.isComplete = false;
                }
            });
        }
        
        private void OnEnemyKilled(string enemyID)
        {
            _objectives.ForEach(obj =>
            {
                // Skip if the objective is not a kill objective
                if (obj.objectiveData.type != ObjectiveType.Kill) return;
                
                var killObj = (KillObjective) obj.objectiveData;
                
                // Skip if the killed enemy is not the required enemy
                if (killObj.enemy.enemyID != enemyID) return;
                obj.currentCount++;
                if (obj.currentCount >= obj.objectiveData.requiredCount)
                {
                    obj.isComplete = true;
                    //unsubscribing from the event because the objective is can only increase
                }
            });
        }
        
        
        private void CheckCompletion()
        {
            bool isAllDone = true;
            foreach (var obj in _objectives)
            {
                if (!obj.isComplete)
                {
                    isAllDone = false;
                    break;
                }
            }

            if (isAllDone)
            {
                Debug.Log($"[QuestInstance] 所有目标完成，任务 {_definition.questName} 完成！");
                
            }
        }
        
        public string GetQuestStatus()
        {
            string status = "";
            //Name
            status += "Quest: \n";
            status += _definition.questName + "\n";
            //Objectives
            status += "Objectives: \n";
            foreach (var obj in _objectives)
            {
                switch (obj.objectiveData.type)
                {
                    case ObjectiveType.Kill:
                        status += "Kill " + ((KillObjective) obj.objectiveData).enemy.enemyID + " " + obj.currentCount + "/" + obj.objectiveData.requiredCount + "\n";
                        break;
                    case ObjectiveType.Collect:
                        status += "Collect " + ((CollectObjective) obj.objectiveData).item.itemID + " " + obj.currentCount + "/" + obj.objectiveData.requiredCount + "\n";
                        break;
                    case ObjectiveType.Explore:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                status += "----------------\n";
            }

            return status;
        }

        // 当任务完成/销毁时，最好取消订阅事件，避免内存泄露
        public void Cleanup()
        {
            if (QuestManager.Instance == null) return;
            QuestManager.Instance.onEnemyKilled -= OnEnemyKilled;
            QuestManager.Instance.onItemCollected -= OnItemCollected;
        }
        
    }
}

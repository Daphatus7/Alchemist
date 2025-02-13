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
        public SimpleQuestDefinition QuestDefinition { get; }
        public abstract QuestType QuestType { get; }
        private QuestState _state; public QuestState QuestState
        {
            get => _state;
            set
            {
                Debug.Log($"[QuestInstance] Quest {_state} -> {value}");
                _state = value;
            }
        }

        private readonly List<QuestObjective> _objectives = new List<QuestObjective>(); public List<QuestObjective> Objectives => _objectives;
        
        public QuestInstance(SimpleQuestDefinition def)
        {
            Debug.Log("Quest created");
            QuestDefinition = def;
            _state = QuestState.NotStarted;
            // For each static ObjectiveData, create a dynamic QuestObjective
            if (QuestManager.Instance != null)
            {
                foreach (var objData in QuestDefinition.objectives)
                {
                    var questObj = new QuestObjective(objData.objectiveData);
                    _objectives.Add(questObj);
                    switch (objData.objectiveData.Type)
                    {
                        case ObjectiveType.Kill:
                            QuestManager.Instance.onEnemyKilled += OnEnemyKilled;
                            break;
                        case ObjectiveType.Collect:
                            QuestManager.Instance.onItemCollected += OnItemCollected;
                            break;
                        case ObjectiveType.Explore:
                            QuestManager.Instance.onAreaEntered += OnEnteringArea;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        
        private void OnItemCollected(string itemID, int totalCount)
        {
            _objectives.ForEach(obj =>
            {
                // Skip if the objective is not a collect objective
                if (obj.objectiveData.Type != ObjectiveType.Collect) return;
                
                var collectObj = (CollectObjective) obj.objectiveData;
                
                // Skip if the collected item is not the required item
                if (collectObj.item.itemID != itemID) return;
                obj.CurrentCount = totalCount;
                if (obj.CurrentCount >= obj.objectiveData.requiredCount)
                {
                    obj.isComplete = true;
                    //considered as completed now, but if the items are removed, the objective will be considered as not completed
                }
                else
                {
                    obj.isComplete = false;
                }
            });
            
            //check if the quest is complete
            CheckCompletion();
        }
        
        private void OnEnemyKilled(string enemyID)
        {
            _objectives.ForEach(obj =>
            {
                // Skip if the objective is not a kill objective
                if (obj.objectiveData.Type != ObjectiveType.Kill) return;
                
                var killObj = (KillObjective) obj.objectiveData;
                
                // Skip if the killed enemy is not the required enemy
                if (killObj.enemy.enemyID != enemyID) return;
                obj.CurrentCount++;
                if (obj.CurrentCount >= obj.objectiveData.requiredCount)
                {
                    obj.isComplete = true;
                    //unsubscribing from the event because the objective is can only increase
                }
            });
        }
        private void OnEnteringArea(string areaID)
        {
            _objectives.ForEach(obj =>
            {
                // Skip if the objective is not a explore objective
                if (obj.objectiveData.Type != ObjectiveType.Explore) return;
                
                var exploreObj = (ExplorationObjective) obj.objectiveData;
                
                // Skip if the explored area is not the required area
                if (exploreObj.areaID != areaID) return;
                obj.isComplete = true;
            });
            CheckCompletion();
        }
        
        
        private void CheckCompletion()
        {
            bool isAllDone = true;
            foreach (var obj in _objectives)
            {
                if (!obj.isComplete)
                {
                    isAllDone = false;
                    _state = QuestState.InProgress;
                    break;
                }
            }

            if (isAllDone)
            {
                _state = QuestState.Completed;
            }
            Debug.Log($"[QuestInstance] 所有目标完成，任务 {QuestDefinition.questName} {_state}");
        }
        
        public string QuestStatus
        {
            get
            {        
                string status = "";
                //Name
                status += "Quest: \n";
                status += QuestDefinition.questName + "\n";
                //Objectives
                status += "Objectives: \n";
                foreach (var obj in _objectives)
                {
                    switch (obj.objectiveData.Type)
                    {
                        case ObjectiveType.Kill:
                            status += "Kill " + ((KillObjective) obj.objectiveData).enemy.enemyID + " " + obj.CurrentCount + "/" + obj.objectiveData.requiredCount + "\n";
                            break;
                        case ObjectiveType.Collect:
                            status += "Collect " + ((CollectObjective) obj.objectiveData).item.itemID + " " + obj.CurrentCount + "/" + obj.objectiveData.requiredCount + "\n";
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
        }
        
        // Unsubscribe from events
        public void Cleanup()
        {
            if (QuestManager.Instance == null) return;
            QuestManager.Instance.onEnemyKilled -= OnEnemyKilled;
            QuestManager.Instance.onItemCollected -= OnItemCollected;
            Debug.Log("QuestInstance cleaned up to prevent memory leak");
        }
    }
    
    public enum QuestType
    {
        Main,
        Side,
        Guild
    }
}

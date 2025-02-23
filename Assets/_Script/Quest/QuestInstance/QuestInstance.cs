// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 43

using System;
using System.Collections.Generic;

using _Script.Quest.QuestDefinition;
using UnityEngine;

namespace _Script.Quest.QuestInstance
{
    /// <summary>
    /// Player holding instance
    /// </summary>
    public abstract class QuestInstance
    {
        public SimpleQuestDefinition QuestDefinition { get; }
        public QuestType QuestType => QuestDefinition.QuestType;

        #region Required to save
        private QuestState _state; public QuestState QuestState
        {
            get => _state;
            set
            {
                Debug.Log($"[QuestInstance] Quest {_state} -> {value}");
                _state = value;
            }
        }
        private readonly List<QuestObjective> _objectives = new (); 
        public List<QuestObjective> Objectives => _objectives;
        #endregion
        
        public QuestInstance(SimpleQuestDefinition def)
        {
            Debug.Log("Quest created");
            QuestDefinition = def;
            _state = QuestState.NotStarted;
            // For each static ObjectiveData, create a dynamic QuestObjective
            SubscribeToTarget(QuestDefinition.objectives);
        }
        
        /// <summary>
        /// Initialize the quest from save
        /// </summary>
        /// <param name="def"></param>
        /// <param name="save"></param>
        public QuestInstance(SimpleQuestDefinition def, QuestSave save)
        {
            Debug.Log("Quest created from save");
            QuestDefinition = def;
            _state = save.questState;
            // For each static ObjectiveData, create a dynamic QuestObjective
            SubscribeToTarget(QuestDefinition.objectives);
        }

        #region Subscriptions

        private void SubscribeToTarget(QuestObjective [] objectives)
        {
            if (QuestManager.Instance != null)
            {
                foreach (var objData in objectives)
                {
                    //Instantiate the objective, runtime data
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
            else
            {
                throw new Exception("QuestManager is not initialized");
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
            Debug.Log("Enemy killed" + enemyID);
            _objectives.ForEach(obj =>
            {
                // Skip if the objective is not a kill objective
                if (obj.objectiveData.Type != ObjectiveType.Kill) return;
                
                var killObj = (KillObjective) obj.objectiveData;
                
                // Skip if the killed enemy is not the required enemy
                if (killObj.EnemyID != enemyID) return;
                obj.CurrentCount++;
                if (obj.CurrentCount >= obj.objectiveData.requiredCount)
                {
                    obj.isComplete = true;
                    //unsubscribing from the event because the objective is can only increase
                }
            });
            CheckCompletion();
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
        }
        
        #endregion

        #region Unsubscribe

        public void Cleanup()
        {
            if (QuestManager.Instance == null) return;
            QuestManager.Instance.onEnemyKilled -= OnEnemyKilled;
            QuestManager.Instance.onItemCollected -= OnItemCollected;
            //Debug.Log("QuestInstance cleaned up to prevent memory leak");
        }

        #endregion
        
        #region Debug

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
                            status += "Kill " + ((KillObjective) obj.objectiveData).EnemyID + " " + obj.CurrentCount + "/" + obj.objectiveData.requiredCount + "\n";
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


        #endregion

        #region Save and Load

        public QuestSave OnSave()
        {
            var questSave = new QuestSave
            {
                questId = QuestDefinition.questID,
                questState = _state,
                objectives = new QuestObjectiveSave[_objectives.Count],
            };
            
            for(var i = 0; i < _objectives.Count; i++)
            {
                questSave.objectives[i] = _objectives[i].OnSave(i);
            }
            return questSave;
        }
        
        /// <summary>
        /// Mapping the save data onto the objectives regardless of the data type
        /// </summary>
        /// <param name="save"></param>
        public void OnLoad(QuestSave save)
        {
            _state = save.questState;
            for (var i = 0; i < save.objectives.Length; i++)
            {
                _objectives[i].OnLoad(save.objectives[i]);
            }
        }

        #endregion
    }

    [Serializable]
    public class QuestSave
    {
        public string questId;
        public QuestState questState;
        //objectives progresses
        public QuestObjectiveSave[] objectives;
    }

    
    public enum QuestType
    {
        Main,
        Side,
        Guild
    }
}

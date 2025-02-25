// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 14

using System;
using System.Collections.Generic;
using System.Linq;
using _Script.Items.Lootable;
using _Script.Managers;
using _Script.Map;
using _Script.NPC.NpcBackend;
using _Script.NPC.NpcBackend.NpcModules;
using _Script.Quest.QuestDefinition;
using _Script.Quest.QuestInstance;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Script.Quest
{
    
    public sealed class QuestManager : PersistentSingleton<QuestManager>, IPlayerQuestSave
    {
        /// <summary>
        /// NPC ID -> Quest Giver Module
        /// </summary>
        private Dictionary<string, INpcQuestModuleHandler> _questModules;
        public event Action<string> onEnemyKilled;
        public event Action<string, int> onItemCollected;
        public event Action<string> onAreaEntered;
        public event Action<QuestInstance.QuestInstance> onQuestCompleted;
        
        [SerializeField] public StorylineChecker storylineChecker;
        
        public void OnDestroy()
        {
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.Unregister<IPlayerQuestSave>();
            }
        }
        
        public void Start()
        {
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.Register<IPlayerQuestSave>(this);
            }
            if(SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.LoadQuestData();
            }
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

        public bool CheckQuestCompletion(QuestInstance.QuestInstance quest)
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
        
        public void CompleteQuest(QuestInstance.QuestInstance currentQuest)
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
            currentQuest.Cleanup();
        }
        
        private void GiveReward(QuestReward reward)
        {
            //Give the player the reward experience
            GameManager.Instance.PlayerCharacter.CurrentRank.AddExperience(reward.experience);
            
            //Drop the reward items
            foreach (var o in reward.items)
            {
                //Debug the item drop
                Debug.Log($"[QuestManager] Dropping {o.amount} {o.item.name}");
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

        private void OnQuestCompleted(QuestInstance.QuestInstance quest)
        {
            onQuestCompleted?.Invoke(quest);
        }


        #region Guild Quests

        private bool _isQuestInitialized;
        private GuildQuestInstance _currentQuest;
        private List<GuildQuestInstance> _availableQuests;

        
        /// <summary>
        /// 仅限于当玩家与NPC进行交互时调用
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public List<GuildQuestInstance> GetAvailableGuildQuests(GuildQuestGiverModule module)
        {
            Debug.Log("GetAvailableGuildQuests");
            //the player has not accepted quest
            if (CurrentQuest == null) //player don't have a active quest
            {
                Debug.Log("No active quest");
                if (_availableQuests == null || _availableQuests.Count == 0)
                {
                    Debug.Log("Generate new quests");
                    //Generate new quests
                    var all = module.AllQuests;
                    // If there are fewer than 3 quests, return all
                    if (all.Count > 3)
                    {
                        //get 3 random quests
                        all = all
                            .OrderBy(_ => Random.value)
                            .Take(3)
                            .ToList();
                    }
                    //generate quest instances based player rank
                    var playerRank = GameManager.Instance.PlayerRank;
                    var q = all.Select(quest => new GuildQuestInstance(quest, playerRank)).ToList();
                    _availableQuests = q;
                }
                return _availableQuests;
            }
            else
            {
                Debug.Log("complete the current quest first");
                return null;
            }
            //this manager runs persistently, so it will not actively delete data
            //only possible cases where the available quests are reset
            //1. there is no quests, because the game hasn't started yet
            //2. the player has completed current quest, and new quests are required to be generated
            //3. Load Saved progress.
            //if has active quest -> don't generate, just tell the player to finish existing one.
            
            //cases where the method is called
            //when the available quests are empty.
        }
        
        public GuildQuestInstance CurrentQuest
        {
            get => _currentQuest;
            set
            {
                if(value == null)
                {
                    Debug.Log("Quest is completed");
                    OnQuestUpdate(_currentQuest, QuestUpdateInstruction.Complete);
                    _currentQuest = null;
                }
                else
                {
                    if (_currentQuest == value)
                    {
                        throw new Exception("how did you assign the same quest?");
                    }
                    else
                    {
                        _currentQuest = value;
                        OnQuestUpdate(_currentQuest, QuestUpdateInstruction.Accept);
                    }
                }
            }
        }
        
        
        /// <summary>
        /// Try to create a new guild quest
        /// If there is an existing quest, return null
        /// </summary>
        /// <param name="questInstance"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public GuildQuestInstance CreateGuildQuest(GuildQuestInstance questInstance)
        {
            //check if there is an existing quest
            if(CurrentQuest != null)
            {
                switch (CurrentQuest.QuestState)
                {
                    case QuestState.Completed:
                        //cannot create a new quest until it is submitted
                        Debug.Log("Quest is completed, cannot create a new quest" +
                                  "until it is submitted");
                        return null;
                    case QuestState.InProgress:
                        Debug.Log("Quest is in progress, cannot create a new quest potential add a new UI to inform the player");
                        return null;
                    case QuestState.NotStarted:
                        return null;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            CurrentQuest = questInstance;
            CurrentQuest.QuestState = QuestState.InProgress;
            MapController.Instance.CreateQuest(CurrentQuest);
            return CurrentQuest;
        }
        
        
        public void CompleteGuildQuest(string questId)
        {
            if (CurrentQuest == null)
            {
                Debug.Log("No quest to complete");
                return;
            }
            if (CurrentQuest.QuestDefinition.questID == questId)
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
            if (CurrentQuest == null)
            {
                Debug.Log("No quest to complete");
                return;
            }
            if (CheckQuestCompletion(CurrentQuest))
            {
                CompleteQuest(CurrentQuest);
                CurrentQuest = null;
                _availableQuests = null;
            }
            else
            {
                Debug.Log("Quest is not completed");
            }
        }
        #region Quest Status Update

        private void OnQuestUpdate(GuildQuestInstance obj, QuestUpdateInstruction instruction)
        {
            switch (instruction)
            {
                case QuestUpdateInstruction.Accept:
                    OnGuildQuestAccepted(obj);
                    break;
                case QuestUpdateInstruction.Complete:
                    OnGuildQuestCompleted(obj);
                    break;
                case QuestUpdateInstruction.Expire:
                    OnGuildQuestExpired(obj);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(instruction), instruction, null);
            }
        }
        
        
        
        private void OnGuildQuestAccepted(GuildQuestInstance obj)
        {
            ServiceLocator.Instance.Get<IPlayerQuestService>().AddGuildQuest(obj);
        }
        
        private void OnGuildQuestCompleted(GuildQuestInstance obj)
        {
            ServiceLocator.Instance.Get<IPlayerQuestService>().RemoveGuildQuest(obj);
        }
        
        private void OnGuildQuestExpired(GuildQuestInstance obj)
        {
            //Debug.Log("Quest Expired");
        }
        

        #endregion
        
        
        #endregion

        public string SaveKey => "QuestManager";
        public object OnSaveData()
        {
            //Save guild quests
            var saveModule = new GuildQuestSaveModule();
            
            if (CurrentQuest != null)
            {
                saveModule.currentQuest = CurrentQuest.OnSave();
            }
            
            //Save available quests - so it can be consistent
            if(_availableQuests != null)
            {
                saveModule.questSaves = new QuestSave[_availableQuests.Count];
                for (var i = 0; i < _availableQuests.Count; i++)
                {
                    if(_availableQuests[i] != null)
                    {
                        saveModule.questSaves[i] = _availableQuests[i].OnSave();
                        Debug.Log("Quest saved" + _availableQuests[i].QuestDefinition.questID);
                    }
                }
            }
            return saveModule;
        }

        public void OnLoadData(object data)
        {
           if (data is GuildQuestSaveModule saveModule)
            {
                if (saveModule.currentQuest != null)
                {
                    var questID = saveModule.currentQuest.questId;
                    if (!string.IsNullOrEmpty(questID))
                    {
                        var questDefinition = DatabaseManager.Instance.GetQuestDefinition(questID);
                        if (questDefinition is GuildQuestDefinition guildQuestDefinition)
                            CurrentQuest = new GuildQuestInstance(guildQuestDefinition, saveModule.currentQuest as GuildQuestSave);
                    }
                    else
                    {
                        Debug.Log("GuildQuestGiverModule.OnLoadData: Invalid quest ID.");
                    }
                    
                }
                _availableQuests = new List<GuildQuestInstance>();
                
                if(saveModule.questSaves != null)
                {
                    for (var i = 0; i < saveModule.questSaves.Length; i++)
                    {
                        var questID = saveModule.questSaves[i].questId;
                        if(!string.IsNullOrEmpty(questID))
                        {
                            var questDefinition =
                                DatabaseManager.Instance.GetQuestDefinition(saveModule.questSaves[i].questId);
                            if (questDefinition is GuildQuestDefinition guildQuestDefinition)
                                _availableQuests.Add(new GuildQuestInstance(guildQuestDefinition,
                                    saveModule.questSaves[i] as GuildQuestSave));
                        }
                        else
                        {
                            Debug.Log("GuildQuestGiverModule.OnLoadData: Invalid quest ID.");
                        }
                    }
                }
            }
            else
            {
                Debug.Log("GuildQuestGiverModule.OnLoadData: Invalid save data type.");
                LoadDefaultData();
            }
            
        }

        public void LoadDefaultData()
        {
            
        }
    }
    
    public enum QuestUpdateInstruction
    {
        Accept,
        Complete,
        Expire
    }
    
    public interface IPlayerQuestSave : ISaveGame
    {
    }
    
    [Serializable]
    public class GuildQuestSaveModule : NpcSaveModule
    {
        public QuestSave [] questSaves; //Save all quest instances
        public QuestSave currentQuest; //Save the current quest
    }
}
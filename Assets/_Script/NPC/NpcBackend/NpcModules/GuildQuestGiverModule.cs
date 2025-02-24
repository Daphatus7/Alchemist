// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 09

using System;
using System.Collections.Generic;
using System.Linq;
using _Script.Managers;
using _Script.Quest;
using _Script.Quest.GuildQuestUI;
using _Script.Quest.QuestDefinition;
using _Script.Quest.QuestInstance;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Script.NPC.NpcBackend.NpcModules
{
    
    /// <summary>
    /// Quest Giver Module
    /// Data 
    /// </summary>
    public class GuildQuestGiverModule : NpcModuleBase, IGuildQuestGiverModuleHandler
    {
        [SerializeField] private string optionName = "Guild Quest";
        public override string ModuleDescription => "Guild Quest Giver Module";
        public override string ModuleName => optionName;

        public override NpcHandlerType HandlerType => NpcHandlerType.GuildQuestGiver;

        [SerializeField] private List<GuildQuestDefinition> allQuests;

        private List<GuildQuestInstance> _availableQuests;
        
        /// <summary>
        /// Quest Generation, set available quests to null to reset
        /// </summary>
        private List<GuildQuestInstance> AvailableQuests
        {
            set
            {
                // Cleanup old quest instances
                foreach (var instance in _availableQuests)
                {
                    instance?.Cleanup();
                }
                
                _availableQuests = value;
            }
            get
            {
                if (_availableQuests == null)
                {
                    var all = allQuests;

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
                    // Shuffle using Random.value and take 3
                    return q;
                }

                return _availableQuests;
            }
        } 
        public List<GuildQuestInstance> GetAvailableQuests => AvailableQuests;

        private GuildQuestInstance _currentGuildQuest;

        public GuildQuestInstance CurrentGuildQuest
        {
            get => _currentGuildQuest;
            set
            {
                //If the current quest is set to null -> for current build, it can only mean the quest is completed
                if(value == null)
                {
                    Debug.Log("Quest is completed");
                    _currentGuildQuest = null;
                }
                else
                {
                    //They are not the same quest
                    if (_currentGuildQuest == value)
                    {
                        throw new Exception("how did you assign the same quest?");
                    }
                    else
                    {
                        _currentGuildQuest = value;
                    }
                }
            }
        }

        public override bool ShouldLoadModule()
        {
            return true;
        }
        public override void LoadNpcModule()
        {
            if (CurrentGuildQuest != null)
            {
                //if true ->
                // and not finished display UI, saying that you need to finish the quest
                if(CurrentGuildQuest.QuestState == QuestState.Completed)
                {
                    Debug.Log("Quest Completed");
                    var handler = ServiceLocator.Instance.Get<IGuildQuestUIHandler>();
                    handler.LoadQuestReward(CurrentGuildQuest, this);
                    Npc.AddMoreUIHandler(handler as IUIHandler);
                }
                else if (CurrentGuildQuest.QuestState == QuestState.InProgress)
                {
                    Debug.Log("Quest In Progress");
                    var handler = ServiceLocator.Instance.Get<IGuildQuestUIHandler>();
                    handler.LoadQuestInProgress(CurrentGuildQuest,this);
                    Npc.AddMoreUIHandler(handler as IUIHandler);
                }
                else
                {
                    Debug.Log(CurrentGuildQuest.QuestState + " is not a valid state");
                    Debug.LogError("Invalid Quest State");
                }
            }
            else
            {
                var handler = ServiceLocator.Instance.Get<IGuildQuestUIHandler>();
                handler.LoadQuestGiver(this);
                Npc.AddMoreUIHandler(handler as IUIHandler);
                
            }
        }

        public override void UnloadNpcModule()
        {
            Debug.LogError("Not implemented");
        }
        
        public void OnAcceptQuest(GuildQuestInstance instance)
        {
            var newQuest = QuestManager.Instance.CreateGuildQuest(instance);
            //If quest is created
            if(newQuest != null)
            {
                CurrentGuildQuest = newQuest;
            }
        }
        
        public void OnQuestComplete()
        {
            Debug.Log("Quest Completed");
            CurrentGuildQuest = null;
            AvailableQuests = null;
        }

        #region Save and Load
        public override void OnLoadData(NpcSaveModule data)
        {
            if (data is GuildQuestSaveModule saveModule)
            {
                // Load the current quest
                if (saveModule.currentQuest != null)
                {
                    var questID = saveModule.currentQuest.questId;
                    if (!string.IsNullOrEmpty(questID))
                    {
                        var questDefinition = DatabaseManager.Instance.GetQuestDefinition(questID);
                        if (questDefinition is GuildQuestDefinition guildQuestDefinition)
                        {
                            CurrentGuildQuest = new GuildQuestInstance(guildQuestDefinition,
                                saveModule.currentQuest as GuildQuestSave);
                            //Error when the player has accepted the quest
                            //but the quest hasn't started
                            //Need to regenerate map or load saved one
                            QuestManager.Instance.CreateGuildQuest(CurrentGuildQuest);
                        }                    
                    }
                    else
                    {
                        Debug.Log("GuildQuestGiverModule.OnLoadData: Invalid quest ID.");
                    }
                    
                }
                AvailableQuests = new List<GuildQuestInstance>();
                
                // Load available quests
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
                            {
                                _availableQuests.Add(new GuildQuestInstance(guildQuestDefinition,
                                    saveModule.questSaves[i] as GuildQuestSave));
                            }
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

        public override NpcSaveModule OnSaveData()
        {
            var saveModule = new GuildQuestSaveModule();
            
            if (_currentGuildQuest != null)
            {
                saveModule.currentQuest = _currentGuildQuest.OnSave();
            }
            if(AvailableQuests != null)
            {
                for (var i = 0; i < AvailableQuests.Count; i++)
                {
                    
                    saveModule.questSaves[i] = AvailableQuests[i].OnSave();
                }
            }
            return saveModule;
        }

        public override void LoadDefaultData()
        {
            // Reset to default state; the getter for AvailableQuests will generate new quests when needed.
            _currentGuildQuest = null;
            AvailableQuests = null;
            Debug.Log("GuildQuestGiverModule loaded default quest data.");
        }
        
        #endregion
    }
    
    [Serializable]
    public class GuildQuestSaveModule : NpcSaveModule
    {
        public QuestSave [] questSaves; //Save all quest instances
        public QuestSave currentQuest; //Save the current quest
    }
    
    public interface IGuildQuestGiverModuleHandler
    {
        void OnAcceptQuest(GuildQuestInstance instance);
        List<GuildQuestInstance> GetAvailableQuests { get; }
        GuildQuestInstance CurrentGuildQuest { get; }
        void OnQuestComplete();
    }
}

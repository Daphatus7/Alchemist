// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 09

using System;
using System.Collections.Generic;
using System.Linq;
using _Script.Character.PlayerRank;
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
        public GuildQuestInstance CurrentGuildQuest => _currentGuildQuest;

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
            if (CurrentGuildQuest != null)
            {
                Debug.Log("There is already a quest in progress");
                return;
            }
            _currentGuildQuest = instance;
            Debug.Log("Accepting Quest let display that there is a active quest and the player should work on it until it expires");
        }
        
        public void OnQuestComplete()
        {
            Debug.Log("Quest Completed");
            _currentGuildQuest = null;
            _availableQuests = null;
        }


        #region Save and Load
        public override void OnLoadData(NpcSaveModule data)
        {
            if (data is GuildQuestSaveModule saveModule)
            {
                if (saveModule.currentQuest != null)
                {
                    var questDefinition = 
                        DatabaseManager.Instance.GetQuestDefinition(saveModule.currentQuest.questId);
                    if (questDefinition is GuildQuestDefinition guildQuestDefinition)
                        _currentGuildQuest = new GuildQuestInstance(guildQuestDefinition, saveModule.currentQuest as GuildQuestSave);
                }
                _availableQuests = new List<GuildQuestInstance>();
                
                if(saveModule.questSaves != null)
                {
                    for (var i = 0; i < saveModule.questSaves.Length; i++)
                    {
                        var questDefinition =
                            DatabaseManager.Instance.GetQuestDefinition(saveModule.questSaves[i].questId); 
                        if (questDefinition is GuildQuestDefinition guildQuestDefinition)
                            _availableQuests.Add(new GuildQuestInstance(guildQuestDefinition, saveModule.questSaves[i] as GuildQuestSave));
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
            for(var i = 0; i < _availableQuests.Count; i++)
            {
                saveModule.questSaves[i] = _availableQuests[i].OnSave();
            }
            return saveModule;
        }

        public override void LoadDefaultData()
        {
            // Reset to default state; the getter for AvailableQuests will generate new quests when needed.
            _currentGuildQuest = null;
            _availableQuests = null;
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

// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 09

using System;
using System.Collections.Generic;
using _Script.Managers;
using _Script.Quest;
using _Script.Quest.GuildQuestUI;
using _Script.Quest.QuestDefinition;
using _Script.Quest.QuestInstance;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

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

        /// <summary>
        /// Quest to be given to the player
        /// </summary>
        [SerializeField] private List<GuildQuestDefinition> allQuests;

        public List<GuildQuestDefinition> AllQuests => allQuests;
        
        private List<GuildQuestInstance> _availableQuests;
        
        /// <summary>
        /// Quest Generation, set available quests to null to reset
        /// </summary>
        private List<GuildQuestInstance> AvailableQuests
        {
            get
            {
                if (QuestManager.Instance != null)
                {
                    return QuestManager.Instance.GetAvailableGuildQuests(this);
                }
                else
                {
                    Debug.LogError("QuestManager is not initialized");
                    return null;
                }
            }
        } 
        public List<GuildQuestInstance> GetAvailableQuests => AvailableQuests;
        
        public GuildQuestInstance CurrentGuildQuest
        {
            get
            {
                if (QuestManager.Instance != null)
                {
                    return QuestManager.Instance.CurrentQuest;
                }
                return null;
            }
        }

        public void OnQuestComplete()
        {
            Debug.Log("Quest completed, but has actual implementation for now," +
                      "could be used for UI or aniamtion purpose");
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
        }
        
        
        #region Save Data
        public override void OnLoadData(NpcSaveModule data)
        {
            
        }

        public override NpcSaveModule OnSaveData()
        {
            return new GuildQuestSaveModule();
        }

        public override void LoadDefaultData()
        {
            
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

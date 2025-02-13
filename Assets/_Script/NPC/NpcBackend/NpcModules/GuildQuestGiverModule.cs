// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 09

using System;
using System.Collections.Generic;
using System.Linq;
using _Script.NPC.NPCFrontend;
using _Script.Quest;
using _Script.Quest.GuildQuestUI;
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

        [SerializeField] private List<GuildQuestDefinition> allQuests;

        private List<GuildQuestDefinition> _availableQuests;
        private List<GuildQuestDefinition> AvailableQuests
        {
            get
            {
                if (_availableQuests == null)
                {
                    var all = allQuests;

                    // If there are fewer than 3 quests, return all
                    if (all.Count <= 3)
                    {
                        return all;
                    }

                    // Shuffle using Random.value and take 3
                    return all
                        .OrderBy(_ => UnityEngine.Random.value)
                        .Take(3)
                        .ToList();
                }

                return _availableQuests;
            }
          
        }

        public List<GuildQuestDefinition> GetAvailableQuests => AvailableQuests;
        
        public override bool ShouldLoadModule()
        {
            return true;
        }

        public override void LoadNpcModule()
        {
            //check if there is an active guild quest
            //if true ->
                // and not finished display UI, saying that you need to finish the quest
                // and finished display UI, saying that you have finished the quest,
            
            //if false -> displaying the normal UI
            
            ServiceLocator.Instance.Get<IGuildQuestUIHandler>().LoadQuestGiver(this);
        }

        public override void UnloadNpcModule()
        {
            Debug.LogError("Not implemented");
        }
        
        public void OnAcceptQuest(GuildQuestDefinition questDefinition)
        {
            Debug.Log("Accepting Quest let display that there is a active quest and the player should work on it until it expires");
        }
    }

    public interface IGuildQuestGiverModuleHandler
    {
        void OnAcceptQuest(GuildQuestDefinition questDefinition);
        List<GuildQuestDefinition> GetAvailableQuests { get; }
    }
}

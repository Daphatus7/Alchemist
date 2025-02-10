// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 09

using System;
using System.Collections.Generic;
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
    public class GuildQuestGiverModule : NpcModuleBase
    {
        [SerializeField] private string optionName = "Guild Quest";
        public override string ModuleDescription => "Guild Quest Giver Module";
        public override string ModuleName => optionName;
        public override NpcHandlerType HandlerType => NpcHandlerType.GuildQuestGiver;

        [SerializeField] private List<GuildQuestDefinition> allQuests;

        private List<GuildQuestDefinition> AvailableQuests
        {
            get
            {
                var availableQuests = new List<GuildQuestDefinition>(allQuests);
                return availableQuests;
            }
        }
        
        public override bool ShouldLoadModule()
        {
            //will check player has completed the previous quest
            return true;
        }

        public override void LoadNpcModule()
        {
            ServiceLocator.Instance.Get<IGuildQuestUIHandler>().LoadQuests(AvailableQuests);
        }

        public override void UnloadNpcModule()
        {
            Debug.LogError("Not implemented");
        }


    }
}

// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 09

using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.NPC.NpcBackend.NpcModules
{
    public class QuestGiverModule : NpcModuleBase, INpcModuleHandler
    {
        
        [SerializeField] private string optionName = "Quest";

        public override NpcHandlerType HandlerType => NpcHandlerType.QuestGiver;
        public override string ModuleDescription => "Quest Giver Module";
        public override string ModuleName => optionName;
    }
}

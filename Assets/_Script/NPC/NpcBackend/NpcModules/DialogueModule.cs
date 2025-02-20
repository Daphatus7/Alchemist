// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 39

using System;
using Sirenix.OdinInspector;

namespace _Script.NPC.NpcBackend.NpcModules
{
    [Serializable]
    public class DialogueModule : NpcModuleBase, INpcModuleHandler
    {
        public override bool ShouldLoadModule()
        {
            return false;
        }

        public override void LoadNpcModule()
        {
            throw new NotImplementedException();
        }

        public override void UnloadNpcModule()
        {
            throw new NotImplementedException();
        }


        [LabelText("Dialogue Lines")]
        [ListDrawerSettings(ShowIndexLabels = true)]
        public override NpcHandlerType HandlerType => NpcHandlerType.Dialogue;

        public override string ModuleDescription => "Dialogue Module";
        public override string ModuleName => "Chat";
        public override void OnLoadData(NpcSaveModule data)
        {
        }

        public override NpcSaveModule OnSaveData()
        {
            return null;
        }

        public override void LoadDefaultData()
        {
        }

        public string[] dialogueLines;

        public void StartDialogue()
        {
            
        }
        
    }
}